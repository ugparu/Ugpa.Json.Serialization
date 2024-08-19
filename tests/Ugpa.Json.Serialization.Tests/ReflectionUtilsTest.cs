using System.Reflection;
using Xunit;
using static Ugpa.Json.Serialization.ReflectionUtils;

namespace Ugpa.Json.Serialization.Tests;

public sealed class ReflectionUtilsTest
{
    private readonly MemberInfo i_a;
    private readonly MemberInfo i_b;

    private readonly MemberInfo c1_a;
    private readonly MemberInfo c1_b;
    private readonly MemberInfo c1_c;
    private readonly MemberInfo c1_d;
    private readonly MemberInfo c1_e;

    private readonly MemberInfo c2_d;
    private readonly MemberInfo c2_e;

    public ReflectionUtilsTest()
    {
        i_a = GetMemberInfo<IFoo, int>(_ => _.A);
        i_b = GetMemberInfo<IFoo, int>(_ => _.B);

        c1_a = GetMemberInfo<Foo1, int>(_ => _.A);
        c1_b = GetMemberInfo<Foo1, int>(_ => _.B);
        c1_c = GetMemberInfo<Foo1, int>(_ => _.C);
        c1_d = GetMemberInfo<Foo1, int>(_ => _.D);
        c1_e = GetMemberInfo<Foo1, int>(_ => _.E);

        c2_d = GetMemberInfo<Foo2, int>(_ => _.D);
        c2_e = GetMemberInfo<Foo2, int>(_ => _.E);
    }

    [Fact]
    public void DeclaringTypeIsValidWheFullyConfiguredTest()
    {
        var map = new MemberInfo[] { i_a, i_b, c1_a, c1_b, c1_c, c1_d, c1_e, c2_d, c2_e };

        Assert.Same(typeof(IFoo), LookupMemberInfo(map, _ => _, i_a).DeclaringType);
        Assert.Same(typeof(IFoo), LookupMemberInfo(map, _ => _, i_b).DeclaringType);

        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c1_a).DeclaringType);
        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c1_b).DeclaringType);
        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c1_c).DeclaringType);
        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c1_d).DeclaringType);
        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c1_e).DeclaringType);

        Assert.Same(typeof(Foo2), LookupMemberInfo(map, _ => _, c2_d).DeclaringType);
        Assert.Same(typeof(Foo2), LookupMemberInfo(map, _ => _, c2_e).DeclaringType);
    }

    [Fact]
    public void DeclaringTypeIsValidForInterfaceMapTest()
    {
        var map = new MemberInfo[] { i_a, i_b };
        Assert.Same(typeof(IFoo), LookupMemberInfo(map, _ => _, c1_a).DeclaringType);
    }

    [Fact]
    public void DeclaringTypeIsValidForOverriddenMemberTest()
    {
        var map = new MemberInfo[] { c1_e };
        Assert.Same(typeof(Foo1), LookupMemberInfo(map, _ => _, c2_e).DeclaringType);
    }

    [Fact]
    public void MemberInfoIsNullForOverlappedMemberTest()
    {
        var map = new MemberInfo[] { c1_d };
        Assert.Null(LookupMemberInfo(map, _ => _, c2_d));
    }

    private interface IFoo
    {
        int A { get; set; }

        int B { get; set; }
    }

    private class Foo1 : IFoo
    {
        public int A { get; set; }

        public int B { get; set; }

        int IFoo.B { get; set; }

        public int C { get; set; }

        public int D { get; set; }

        public virtual int E { get; set; }
    }

    private class Foo2 : Foo1
    {
        public new int D { get; set; }

        public override int E { get; set; }
    }
}
