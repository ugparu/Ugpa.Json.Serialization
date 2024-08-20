using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ugpa.Json.Serialization.Properties;

namespace Ugpa.Json.Serialization;

internal static class ReflectionUtils
{
    public static MemberInfo GetMemberInfo<T, TProp>(Expression<Func<T, TProp>> property)
    {
        if (property.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException(Resources.ReflectionUtils_NotMemberExpression);
        }

        if (memberExpression.Expression != property.Parameters[0])
        {
            throw new InvalidOperationException(Resources.ReflectionUtils_ParameterNotMemberOwner);
        }

        var reflectedType = property.Type.GetGenericArguments()[0];

        if (reflectedType == memberExpression.Member.DeclaringType)
        {
            return memberExpression.Member;
        }
        else
        {
            var member = reflectedType.GetProperty(memberExpression.Member.Name);
            if (member is null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.ReflectionUtils_UnableToResolveMember,
                    memberExpression.Member.Name,
                    reflectedType.AssemblyQualifiedName));
            }
            else if (member.DeclaringType != reflectedType)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.ReflectionUtils_ReflectedTypeNotMemberOwner,
                    member.Name,
                    reflectedType.AssemblyQualifiedName,
                    member.DeclaringType.AssemblyQualifiedName));
            }
            else
            {
                return member;
            }
        }
    }

    public static bool LookupMemberInfo<T>(Dictionary<Type, T> configuration, Func<T, IEnumerable<MemberInfo>> getMembers, MemberInfo member, Func<T, MemberInfo, bool> onFound)
    {
        // Trying to find explicit property configuration.
        if (configuration.TryGetValue(member.DeclaringType, out var typeInfo) && onFound(typeInfo, member))
            return true;

        if (member is PropertyInfo property)
        {
            // Trying to find property configuration in base classes.
            var baseType = property.DeclaringType.BaseType;
            while (baseType is not null)
            {
                if (configuration.TryGetValue(baseType, out var baseTypeInfo))
                {
                    var baseProperty = getMembers(baseTypeInfo)
                        .OfType<PropertyInfo>()
                        .FirstOrDefault(_ =>
                            _.Name == property.Name &&
                            _.GetMethod.GetBaseDefinition().DeclaringType == property.GetMethod.GetBaseDefinition().DeclaringType);

                    if (baseProperty is not null && onFound(baseTypeInfo, baseProperty))
                        return true;
                }

                baseType = baseType.BaseType;
            }

            // Trying to find property configuration from interfaces.
            foreach (var i in property.DeclaringType.GetInterfaces())
            {
                if (configuration.TryGetValue(i, out var interfaceInfo))
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
                                null)[0];

                        if (onFound(interfaceInfo, iProperty))
                            return true;
                    }
                }
            }
        }

        return false;
    }

    public static T? LookupMemberInfo<T>(IEnumerable<T> configuration, Func<T, MemberInfo> getMember, MemberInfo member)
    {
        var typeMap = configuration
            .GroupBy(m => getMember(m).DeclaringType)
            .ToDictionary(g => g.Key);

        // Trying to find explicit property configuration.
        if (typeMap.TryGetValue(member.DeclaringType, out var typeInfo))
        {
            var item = typeInfo.FirstOrDefault(i => MemberInfoEqualityComparer.Value.Equals(member, getMember(i)));
            if (item is not null)
            {
                return item;
            }
        }

        if (member is PropertyInfo property)
        {
            // Trying to find property configuration in base classes.
            var baseType = property.DeclaringType.BaseType;
            while (baseType is not null)
            {
                if (typeMap.TryGetValue(baseType, out var baseTypeInfo))
                {
                    var baseProperty = baseTypeInfo
                        .FirstOrDefault(_ =>
                            getMember(_) is PropertyInfo p &&
                            p.Name == property.Name &&
                            p.GetMethod.GetBaseDefinition().DeclaringType == property.GetMethod.GetBaseDefinition().DeclaringType);

                    if (baseProperty is not null)
                    {
                        return baseProperty;
                    }
                }

                baseType = baseType.BaseType;
            }

            // Trying to find property configuration from interfaces.
            foreach (var @interface in property.DeclaringType.GetInterfaces())
            {
                if (typeMap.TryGetValue(@interface, out var interfaceInfo))
                {
                    var map = property.DeclaringType.GetInterfaceMap(@interface);
                    var index = Array.IndexOf(map.TargetMethods, property.GetMethod);

                    if (index != -1)
                    {
                        var iProperty = @interface
                            .FindMembers(
                                MemberTypes.Property,
                                BindingFlags.Instance | BindingFlags.Public,
                                (p, _) => ((PropertyInfo)p).GetMethod == map.InterfaceMethods[index],
                                null)[0];

                        var item = interfaceInfo.FirstOrDefault(i => MemberInfoEqualityComparer.Value.Equals(getMember(i), iProperty));
                        if (item is not null)
                        {
                            return item;
                        }
                    }
                }
            }
        }

        return default;
    }
}
