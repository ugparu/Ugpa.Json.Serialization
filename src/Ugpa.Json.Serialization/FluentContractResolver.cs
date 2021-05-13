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
                    _.Key != member.DeclaringType &&
                    (_.Key.IsAssignableFrom(member.DeclaringType) || member.DeclaringType.IsAssignableFrom(_.Key)))
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

            if (!properties.TryGetValue(member.DeclaringType, out var typeInfo))
            {
                typeInfo = new();
                properties[member.DeclaringType] = typeInfo;
            }

            var propertyInfo = typeInfo.Where(_ => _.Value.Name == name).ToArray();
            if (propertyInfo.Any())
            {
                throw new ArgumentException(string.Format(
                    Resources.FluentContractResolver_PropertyNameConflict,
                    member.DeclaringType,
                    name,
                    propertyInfo[0].Key.Name));
            }

            typeInfo.Add(member, (name, isRequired));
        }

        public void SetFactory<T>(Func<T> factory)
            where T : class
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            defaultCreators[typeof(T)] = factory;
        }

        public void SetFactory<T>(Func<object[], T> factory)
            where T : class
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            overrideCreators[typeof(T)] = new ObjectConstructor<object>(factory!);
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

            var configuredMembers = properties
                .Where(_ => _.Key.IsAssignableFrom(objectType))
                .SelectMany(_ => _.Value.Keys)
                .ToArray();

            members.AddRange(configuredMembers.Except(members));

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (properties.TryGetValue(member.DeclaringType, out var typeInfo) && typeInfo.TryGetValue(member, out var propertyInfo))
            {
                property.PropertyName = propertyInfo.Name;
                property.Required = propertyInfo.IsRequired switch
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
    }
}
