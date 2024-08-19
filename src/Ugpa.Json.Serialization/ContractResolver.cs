using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MemberData = (string Name, bool IsRequired, System.Predicate<object>? SerializeCondition);

namespace Ugpa.Json.Serialization;

internal sealed class ContractResolver : DefaultContractResolver
{
    private readonly Dictionary<Type, Dictionary<MemberInfo, MemberData>> properties = new();
    private readonly Dictionary<Type, HashSet<MemberInfo>> ignored = new();
    private readonly Dictionary<Type, Func<object>> defaultCreators = new();
    private readonly Dictionary<Type, ObjectConstructor<object>> overrideCreators = new();

    public bool AllowNullValues { get; set; } = true;

    public void SetDefaultCreator(Type type, Func<object> factory)
        => defaultCreators[type] = factory;

    public void SetOverrideCreator(Type type, ObjectConstructor<object> factory)
        => overrideCreators[type] = factory;

    public void AddPropertyInfo(Type type, MemberInfo member, string name, bool isRequired, Predicate<object>? serializeCondition)
    {
        if (!properties.TryGetValue(type, out var data))
        {
            data = new(MemberInfoEqualityComparer.Value);
            properties.Add(type, data);
        }

        data.Add(member, (name, isRequired, serializeCondition));
    }

    public void SkipProperty(Type type, MemberInfo member)
    {
        if (!ignored.TryGetValue(type, out var data))
        {
            data = new();
            ignored.Add(type, data);
        }

        data.Add(member);
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

        var fieldsAndProperties = objectType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m is FieldInfo or PropertyInfo);

        var map = properties.SelectMany(p => p.Value).Select(p => new { Member = p.Key, Config = p.Value }).ToList();
        var configuredMembers = fieldsAndProperties
            .Select(i => ReflectionUtils.LookupMemberInfo(map, _ => _.Member, i))
            .Where(_ => _ is not null)
            .Select(m => m!.Member)
            .ToList();

        foreach (var member in configuredMembers)
        {
            if (!members.Exists(_ => MemberInfoEqualityComparer.Value.Equals(_, member)))
            {
                // Checking for explicit interface implementation.
                if (member.DeclaringType.IsInterface && member is PropertyInfo prop)
                {
                    var matchingProperty = fieldsAndProperties
                        .OfType<PropertyInfo>()
                        .FirstOrDefault(p => p != prop && p.Name == prop.Name && p.GetMethod != prop.GetMethod);

                    if (matchingProperty is null)
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

        var ignoredList = ignored.SelectMany(p => p.Value).ToList();
        foreach (var member in members.Where(m => ReflectionUtils.LookupMemberInfo(ignoredList, _ => _, m) is not null).ToList())
        {
            members.Remove(member);
        }

        return members;
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var config = properties.SelectMany(p => p.Value).Select(p => new { p.Key, p.Value }).ToList();
        var match = ReflectionUtils.LookupMemberInfo(config, _ => _.Key, member);

        var property = base.CreateProperty(member, memberSerialization);

        if (match?.Value is { } data)
        {
            property.PropertyName = data.Name;

            property.Required = data.IsRequired switch
            {
                true => Required.Always,
                false when AllowNullValues => Required.Default,
                false => Required.DisallowNull
            };

            if (data.SerializeCondition is not null)
            {
                property.ShouldSerialize = data.SerializeCondition;
            }
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
