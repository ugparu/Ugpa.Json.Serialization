using System.Collections.Generic;
using System.Reflection;

namespace Ugpa.Json.Serialization;

internal sealed class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
{
    public static IEqualityComparer<MemberInfo> Value { get; } = new MemberInfoEqualityComparer();

    public bool Equals(MemberInfo x, MemberInfo y)
    {
        return
            x.Name == y.Name &&
            x.DeclaringType == y.DeclaringType &&
            x.GetType() == y.GetType();
    }

    public int GetHashCode(MemberInfo obj)
    {
        var hash = obj.GetType().GetHashCode();
        hash = (hash * 37) + obj.DeclaringType.GetHashCode();
        hash = (hash * 37) + obj.Name.GetHashCode();
        return hash;
    }
}
