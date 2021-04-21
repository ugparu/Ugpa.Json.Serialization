﻿using System;
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
        private readonly Dictionary<Type, Dictionary<MemberInfo, (string name, bool isRequired)>> properties =
            new Dictionary<Type, Dictionary<MemberInfo, (string name, bool isRequired)>>();

        public bool AllowNullValues { get; set; } = true;

        public void AddProperty(MemberInfo member, string name, bool isRequired)
        {
            var inheritanceProperties = properties
                .Where(_ =>
                    _.Key != member.DeclaringType &&
                    (_.Key.IsAssignableFrom(member.DeclaringType) || member.DeclaringType.IsAssignableFrom(_.Key)))
                .SelectMany(_ => _.Value)
                .Where(_ => _.Value.name == name)
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
                typeInfo = new Dictionary<MemberInfo, (string name, bool isRequired)>();
                properties[member.DeclaringType] = typeInfo;
            }

            var propertyInfo = typeInfo.Where(_ => _.Value.name == name).ToArray();
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
                property.PropertyName = propertyInfo.name;
                property.Required = propertyInfo.isRequired switch
                {
                    true => Required.Always,
                    false when AllowNullValues => Required.Default,
                    false => Required.DisallowNull
                };
            }

            if (member is PropertyInfo propInfo)
            {
                if (!property.Readable && propInfo.CanRead)
                    property.Readable = true;

                if (!property.Writable && propInfo.CanWrite)
                    property.Writable = true;
            }

            return property;
        }
    }
}
