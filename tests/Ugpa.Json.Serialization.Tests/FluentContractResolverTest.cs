using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Ugpa.Json.Serialization.Tests
{
    public sealed class FluentContractResolverTest
    {
        [Fact]
        public void DefaultContractIsValid()
        {
            var resolver = new FluentContractResolver();

            var contract = resolver.ResolveContract(typeof(TestObjectA));
            Assert.IsType<JsonObjectContract>(contract);

            var objContract = (JsonObjectContract)contract;
            Assert.Equal(3, objContract.Properties.Count);

            Assert.Equal(nameof(TestObjectA.Property1), objContract.Properties[nameof(TestObjectA.Property1)].UnderlyingName);
            Assert.Equal(Required.Default, objContract.Properties[nameof(TestObjectA.Property1)].Required);
            Assert.True(objContract.Properties[nameof(TestObjectA.Property1)].Writable);

            Assert.Equal(nameof(TestObjectA.Property2), objContract.Properties[nameof(TestObjectA.Property2)].UnderlyingName);
            Assert.Equal(Required.Default, objContract.Properties[nameof(TestObjectA.Property2)].Required);
            Assert.True(objContract.Properties[nameof(TestObjectA.Property2)].Writable);

            Assert.Equal(nameof(TestObjectA.Property3), objContract.Properties[nameof(TestObjectA.Property3)].UnderlyingName);
            Assert.Equal(Required.Default, objContract.Properties[nameof(TestObjectA.Property3)].Required);
            Assert.False(objContract.Properties[nameof(TestObjectA.Property3)].Writable);
        }

        [Fact]
        public void ErrorOnDuplicatePropertyConfiguration()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false));
        }

        [Fact]
        public void ErrorOnPropertyNameConflictConfiguration()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property2)), "propA", false));
        }

        [Fact]
        public void ConfiguredPropertyNameIsValid()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property2)), "propB", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property3)), "propC", false);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(nameof(TestObjectA.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property2), contract.Properties["propB"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property3), contract.Properties["propC"].UnderlyingName);
        }

        [Fact]
        public void PropertyRequiredAllowNullIsValid()
        {
            var resolver = new FluentContractResolver { AllowNullValues = true };
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property2)), "propB", true);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(Required.Default, contract.Properties["propA"].Required);
            Assert.Equal(Required.Always, contract.Properties["propB"].Required);
        }

        [Fact]
        public void PropertyRequiredDisallowNullIsValid()
        {
            var resolver = new FluentContractResolver { AllowNullValues = false };
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property2)), "propB", true);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(Required.DisallowNull, contract.Properties["propA"].Required);
            Assert.Equal(Required.Always, contract.Properties["propB"].Required);
        }

        [Fact]
        public void ConfiguredPropertiesInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectB).GetProperty(nameof(TestObjectB.Property4)), "propD", false);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectB));

            Assert.Equal(5, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property2), contract.Properties[nameof(TestObjectA.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property3), contract.Properties[nameof(TestObjectA.Property3)].UnderlyingName);

            Assert.Equal(nameof(TestObjectB.Property4), contract.Properties["propD"].UnderlyingName);
            Assert.Equal(nameof(TestObjectB.Property5), contract.Properties[nameof(TestObjectB.Property5)].UnderlyingName);
        }

        [Fact]
        public void TypesWithSameNamePropertiesConfiguresSuccessfully()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectC).GetProperty(nameof(TestObjectC.Property1)), "propA", false);
        }

        [Fact]
        public void ErrorOnDerivedClassPropertyNameConflict()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectB).GetProperty(nameof(TestObjectB.Property4)), "propA", false));

            resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectB).GetProperty(nameof(TestObjectB.Property4)), "propA", false);
            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false));
        }

        [Fact]
        public void InternalPropertiesConfiguredSuccessfully()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectX).GetProperty(nameof(TestObjectX.PropertyX), BindingFlags.Instance | BindingFlags.NonPublic), "x", false);
            resolver.AddProperty(typeof(TestObjectY).GetProperty(nameof(TestObjectY.PropertyY), BindingFlags.Instance | BindingFlags.NonPublic), "y", true);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectX));
            Assert.Single(contract.Properties);
            Assert.Equal(nameof(TestObjectX.PropertyX), contract.Properties["x"].UnderlyingName);
            Assert.True(contract.Properties["x"].Readable);

            contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectY));
            Assert.Equal(2, contract.Properties.Count);
            Assert.Equal(nameof(TestObjectX.PropertyX), contract.Properties["x"].UnderlyingName);
            Assert.Equal(nameof(TestObjectY.PropertyY), contract.Properties["y"].UnderlyingName);
            Assert.True(contract.Properties["x"].Readable);
            Assert.True(contract.Properties["y"].Readable);
        }

        [Fact]
        public void CustomDefaultConstructorSet()
        {
            var resolver = new FluentContractResolver();
            var factory = (Func<TestObjectZ>)(() => new TestObjectZ(1));
            resolver.SetFactory(factory);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            Assert.Same(factory, contract.DefaultCreator);
        }

        [Fact]
        public void CustomOverrideConstructorSet()
        {
            var resolver = new FluentContractResolver();
            var factory = (Func<object[], TestObjectZ>)(_ => new TestObjectZ((int)_[0]));
            resolver.SetFactory(factory);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            Assert.Same(factory, contract.OverrideCreator.Target);
        }

        #region Тестовые объекты

        private class TestObjectA
        {
            public int Property1 { get; set; }

            public int? Property2 { get; private set; }

            public string Property3 { get; }
        }

        private class TestObjectB : TestObjectA
        {
            public bool Property4 { get; set; }

            public bool Property5 { get; set; }
        }

        private class TestObjectC
        {
            public int Property1 { get; set; }
        }

        private class TestObjectX
        {
            internal int PropertyX { get; set; }
        }

        private class TestObjectY : TestObjectX
        {
            internal IEnumerable<int> PropertyY { get; } = new List<int>();
        }

        private class TestObjectZ
        {
            public TestObjectZ(int ctorParameter)
            {
            }
        }

        #endregion
    }
}
