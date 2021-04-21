﻿using Xunit;

namespace Ugpa.Json.Serialization.Tests
{
    public sealed class FluentSerializationBinderTest
    {
        [Fact]
        public void DefaultBindingMatchTypeName()
        {
            var binder = new FluentSerializationBinder();

            binder.BindToName(typeof(TestObjectA), out var asmA, out var typeNameA);
            binder.BindToName(typeof(TestObjectB), out var asmB, out var typeNameB);

            Assert.Equal(typeof(TestObjectA).Assembly.FullName, asmA);
            Assert.Equal(typeof(TestObjectA).FullName, typeNameA);
            Assert.Equal(typeof(TestObjectB).Assembly.FullName, asmB);
            Assert.Equal(typeof(TestObjectB).FullName, typeNameB);

            var typeA = binder.BindToType(asmA, typeNameA);
            var typeB = binder.BindToType(asmB, typeNameB);

            Assert.Equal(typeof(TestObjectA), typeA);
            Assert.Equal(typeof(TestObjectB), typeB);
        }

        [Fact]
        public void ConfiguredBindingIsValid()
        {
            var binder = new FluentSerializationBinder();

            binder.AddBinding(typeof(TestObjectA), "objA");
            binder.AddBinding(typeof(TestObjectB), "objB");

            binder.BindToName(typeof(TestObjectA), out var asmA, out var typeNameA);
            binder.BindToName(typeof(TestObjectB), out var asmB, out var typeNameB);

            Assert.Null(asmA);
            Assert.Equal("objA", typeNameA);
            Assert.Null(asmB);
            Assert.Equal("objB", typeNameB);

            var typeA = binder.BindToType(null, "objA");
            var typeB = binder.BindToType(null, "objB");

            Assert.Equal(typeof(TestObjectA), typeA);
            Assert.Equal(typeof(TestObjectB), typeB);
        }

        private sealed class TestObjectA
        {
        }

        private sealed class TestObjectB
        {
        }
    }
}
