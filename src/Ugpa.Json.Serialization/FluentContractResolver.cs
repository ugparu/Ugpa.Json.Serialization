using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ugpa.Json.Serialization.Properties;

namespace Ugpa.Json.Serialization
{
    internal sealed class FluentContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, Dictionary<MemberInfo, (string Name, bool IsRequired)>> properties = new();
        private readonly Dictionary<Type, Func<object>> defaultCreators = new();
        private readonly Dictionary<Type, ObjectConstructor<object>> overrideCreators = new();

        public bool AllowNullValues { get; set; } = true;

        public void AddProperty(MemberInfo member, string name, bool isRequired)
        {
            var inheritanceProperties = properties
                .Where(_ =>
                    _.Key != member.ReflectedType &&
                    (_.Key.IsAssignableFrom(member.ReflectedType) || member.ReflectedType.IsAssignableFrom(_.Key)))
                .SelectMany(_ => _.Value)
                .Where(_ => _.Value.Name == name)
                .ToArray();

            if (inheritanceProperties.Any())
            {
                throw new ArgumentException(string.Format(
                    Resources.FluentContractResolver_InheritancePropertyNameConflict,
                    member.DeclaringType,
                    name,
                    inheritanceProperties[0].Key.DeclaringType,
                    inheritanceProperties[0].Key.Name));
            }

            if (!properties.TryGetValue(member.ReflectedType, out var typeInfo))
            {
                typeInfo = new();
                properties[member.ReflectedType] = typeInfo;
            }

            var propertyInfo = typeInfo.Where(_ => _.Value.Name == name).ToArray();
            if (propertyInfo.Any())
            {
                throw new ArgumentException(string.Format(
                    Resources.FluentContractResolver_PropertyNameConflict,
                    member.ReflectedType,
                    name,
                    propertyInfo[0].Key.Name));
            }

            typeInfo.Add(member, (name, isRequired));
        }

        public void SetFactory<T>(Func<T> factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            defaultCreators[typeof(T)] = () => factory()!;
        }

        public void SetFactory<T>(Func<object[], T> factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            overrideCreators[typeof(T)] = new ObjectConstructor<object>(_ => factory(_));
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            if (defaultCreators.TryGetValue(objectType, out var defaultCreator))
            {
                contract.DefaultCreator = defaultCreator;
            }

            if (contract is JsonObjectContract objContract && overrideCreators.TryGetValue(objectType, out var overrideCreator))
            {
                objContract.OverrideCreator = overrideCreator;
            }

            return contract;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);

            do
            {
                if (properties.TryGetValue(objectType, out var cfg))
                {
                    foreach (var member in cfg.Keys)
                    {
                        if (!members.Contains(member))
                        {
                            if (member is PropertyInfo property)
                            {
                                var overridenMember = members
                                    .OfType<PropertyInfo>()
                                    .FirstOrDefault(_ => _.GetMethod.GetBaseDefinition() == property.GetMethod.GetBaseDefinition());

                                if (overridenMember is null)
                                {
                                    members.Add(member);
                                }
                            }
                            else
                            {
                                members.Add(member);
                            }
                        }
                    }
                }

                objectType = objectType.BaseType;
            }
            while (objectType is not null);

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (FindPropertyConfiguration(member, out var name, out var isRequired))
            {
                property.PropertyName = name;
                property.Required = isRequired switch
                {
                    true => Required.Always,
                    false when AllowNullValues => Required.Default,
                    false => Required.DisallowNull
                };
            }

            if (member is PropertyInfo propInfo)
            {
                if (!property.Readable && propInfo.CanRead)
                {
                    property.Readable = true;
                }

                if (!property.Writable && propInfo.CanWrite)
                {
                    property.Writable = true;
                }
            }

            return property;
        }

        private bool FindPropertyConfiguration(MemberInfo member, out string? name, out bool isRequired)
        {
            // Trying to find explicit property configuration.
            if (properties.TryGetValue(member.ReflectedType, out var typeInfo) && typeInfo.TryGetValue(member, out var propertyInfo))
            {
                name = propertyInfo.Name;
                isRequired = propertyInfo.IsRequired;
                return true;
            }

            if (member is PropertyInfo property)
            {
                // Trying to find property configuration in base classes.
                var baseType = property.ReflectedType.BaseType;
                while (baseType is not null)
                {
                    if (properties.TryGetValue(baseType, out var baseTypeInfo))
                    {
                        var baseProperty = baseTypeInfo.Keys
                            .OfType<PropertyInfo>()
                            .FirstOrDefault(_ => _.GetMethod.GetBaseDefinition() == property.GetMethod.GetBaseDefinition());

                        if (baseProperty is not null)
                        {
                            var basePropertyInfo = baseTypeInfo[baseProperty];
                            name = basePropertyInfo.Name;
                            isRequired = basePropertyInfo.IsRequired;
                            return true;
                        }
                    }

                    baseType = baseType.BaseType;
                }

                // Trying to find property configuration from interfaces.
                foreach (var i in property.ReflectedType.GetInterfaces())
                {
                    if (properties.TryGetValue(i, out var interfaceInfo))
                    {
                        var map = property.DeclaringType.GetInterfaceMap(i);
                        var index = Array.IndexOf(map.TargetMethods, property.GetMethod);

                        if (index != -1)
                        {
                            var iProperty = i
                                .FindMembers(
                                    MemberTypes.Property,
                                    BindingFlags.Instance | BindingFlags.Public,
                                    (p, _) => ((PropertyInfo)p).GetMethod == map.InterfaceMethods[index],
                                    null)
                                .First();

                            if (interfaceInfo.TryGetValue(iProperty, out var iPropertyInfo))
                            {
                                name = iPropertyInfo.Name;
                                isRequired = iPropertyInfo.IsRequired;
                                return true;
                            }
                        }
                    }
                }
            }

            name = default;
            isRequired = default;
            return false;
        }
    }
}
