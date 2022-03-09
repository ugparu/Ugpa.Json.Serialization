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
        public void DuplicatePropertySkipConfigurationSuccess()
        {
            var resolver = new FluentContractResolver();
            var property = typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1));
            resolver.SkipProperty(property);
            resolver.SkipProperty(property);
        }

        [Fact]
        public void ErrorOnPropertyMultipleConfigurations()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA1", false);

            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA2", false));
        }

        [Fact]
        public void ErrorOnPropertyMultipleConfigurationsForDerivedType()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            Assert.Throws<ArgumentException>(
                () => resolver.AddProperty(typeof(TestObjectB).GetProperty(nameof(TestObjectA.Property1)), "propA", false));
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

            Assert.Equal(nameof(TestObjectB.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectB.Property2), contract.Properties[nameof(TestObjectA.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectB.Property3), contract.Properties[nameof(TestObjectA.Property3)].UnderlyingName);
            Assert.Equal(nameof(TestObjectB.Property4), contract.Properties["propD"].UnderlyingName);
            Assert.Equal(nameof(TestObjectB.Property5), contract.Properties[nameof(TestObjectB.Property5)].UnderlyingName);

            Assert.Equal(typeof(TestObjectA), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA.Property2)].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA.Property3)].DeclaringType);
            Assert.Equal(typeof(TestObjectB), contract.Properties["propD"].DeclaringType);
            Assert.Equal(typeof(TestObjectB), contract.Properties[nameof(TestObjectB.Property5)].DeclaringType);
        }

        [Fact]
        public void ConfiguredSkipedPropertiesInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)));
            resolver.SkipProperty(typeof(TestObjectB).GetProperty(nameof(TestObjectB.Property4)));

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectB));

            Assert.Equal(3, contract.Properties.Count);
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
            Assert.False(contract.Properties.Contains(nameof(TestObjectB.Property4)));
        }

        [Fact]
        public void OverridenPropertyConfigurationInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA2)));

            Assert.Equal(3, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA2.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA2.Property2), contract.Properties[nameof(TestObjectA2.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectA2.Property3), contract.Properties[nameof(TestObjectA2.Property3)].UnderlyingName);

            Assert.Equal(typeof(TestObjectA2), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA2.Property2)].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA2.Property3)].DeclaringType);
        }

        [Fact]
        public void OverridenSkipedPropertyConfigurationInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)));

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA2)));

            Assert.Equal(2, contract.Properties.Count);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA2.Property1)));
        }

        [Fact]
        public void OverridenPropertyIntermediateConfigurationInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA2).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA22)));

            Assert.Equal(3, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA22.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA22.Property2), contract.Properties[nameof(TestObjectA2.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectA22.Property3), contract.Properties[nameof(TestObjectA2.Property3)].UnderlyingName);

            Assert.Equal(typeof(TestObjectA22), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA2.Property2)].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA2.Property3)].DeclaringType);
        }

        [Fact]
        public void OverridenSkipedPropertyIntermediateConfigurationInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(TestObjectA2).GetProperty(nameof(TestObjectA.Property1)));

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA22)));

            Assert.Equal(2, contract.Properties.Count);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA22.Property1)));
        }

        [Fact]
        public void HiddenPropertyConfigurationInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA3)));
            Assert.Equal(4, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA3.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA3.Property1), contract.Properties[nameof(TestObjectA3.Property1)].UnderlyingName);

            Assert.Equal(typeof(TestObjectA), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA3), contract.Properties[nameof(TestObjectA3.Property1)].DeclaringType);
        }

        [Fact]
        public void HiddenPropertyOverlapsSkipedProperty()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(TestObjectA3).GetProperty(nameof(TestObjectA3.Property1)));

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA3)));
            Assert.Equal(3, contract.Properties.Count);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA3.Property1)].DeclaringType);
        }

        [Fact]
        public void ConfiguredInterfacePropertiesInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(ITestObject).GetProperty(nameof(ITestObject.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property3)), "propC", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Equal(3, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property2), contract.Properties[nameof(TestObjectA.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property3), contract.Properties["propC"].UnderlyingName);

            Assert.Equal(typeof(TestObjectA), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA.Property2)].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties["propC"].DeclaringType);
        }

        [Fact]
        public void ConfiguredInterfaceSkipedPropertiesInherits()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(ITestObject).GetProperty(nameof(ITestObject.Property1)));
            resolver.SkipProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property3)));

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Single(contract.Properties);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property3)));
        }

        [Fact]
        public void MultipleInterfacesPropertiesResolverCorrectly()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(ITestObject).GetProperty(nameof(ITestObject.Property1)), "prop1", false);
            resolver.AddProperty(typeof(ITestObject2).GetProperty(nameof(ITestObject2.Property4)), "prop4", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectB)));

            Assert.Equal(5, contract.Properties.Count);
            Assert.Equal(nameof(ITestObject.Property1), contract.Properties["prop1"].UnderlyingName);
            Assert.Equal(nameof(ITestObject2.Property4), contract.Properties["prop4"].UnderlyingName);
        }

        [Fact]
        public void OverridenPropertyConfigurationOverlap()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA2).GetProperty(nameof(TestObjectA2.Property1)), "propA2", false);

            var contractA = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));
            Assert.Equal(3, contractA.Properties.Count);
            Assert.Equal(nameof(TestObjectA.Property1), contractA.Properties["propA"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA), contractA.Properties["propA"].DeclaringType);

            var contractA2 = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA2)));
            Assert.Equal(3, contractA2.Properties.Count);
            Assert.Equal(nameof(TestObjectA2.Property1), contractA2.Properties["propA2"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA2), contractA2.Properties["propA2"].DeclaringType);
        }

        [Fact]
        public void OverridenSkipedPropertyConfigurationOverlap()
        {
            var resolver = new FluentContractResolver();
            resolver.SkipProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)));
            resolver.SkipProperty(typeof(TestObjectA2).GetProperty(nameof(TestObjectA2.Property1)));

            var contractA = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));
            Assert.Equal(2, contractA.Properties.Count);
            Assert.False(contractA.Properties.Contains(nameof(TestObjectA.Property1)));

            var contractA2 = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA2)));
            Assert.Equal(2, contractA2.Properties.Count);
            Assert.False(contractA2.Properties.Contains(nameof(TestObjectA2.Property1)));
        }

        [Fact]
        public void HiddenPropertyConfigurationOverlap()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA", false);
            resolver.AddProperty(typeof(TestObjectA3).GetProperty(nameof(TestObjectA3.Property1)), "propA2", false);

            var contractA = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));
            Assert.Equal(3, contractA.Properties.Count);
            Assert.Equal(nameof(TestObjectA.Property1), contractA.Properties["propA"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA), contractA.Properties["propA"].DeclaringType);

            var contractA3 = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA3)));
            Assert.Equal(4, contractA3.Properties.Count);
            Assert.Equal(nameof(TestObjectA3.Property1), contractA3.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA3.Property1), contractA3.Properties["propA2"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA), contractA3.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA3), contractA3.Properties["propA2"].DeclaringType);
        }

        [Fact]
        public void InterfacePropertyConfigurationOverlap()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(ITestObject).GetProperty(nameof(ITestObject.Property1)), "propA1", false);
            resolver.AddProperty(typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1)), "propA2", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));
            Assert.Equal(3, contract.Properties.Count);
            Assert.Equal(nameof(ITestObject.Property1), contract.Properties["propA2"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA), contract.Properties["propA2"].DeclaringType);
        }

        [Fact]
        public void SkippedPropertyPriorityOverConfiguration()
        {
            var resolver = new FluentContractResolver();
            var property = typeof(TestObjectA).GetProperty(nameof(TestObjectA.Property1));
            resolver.AddProperty(property, "propA1", false);
            resolver.SkipProperty(property);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Equal(2, contract.Properties.Count);
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
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
            Assert.Equal(typeof(TestObjectX), contract.Properties["x"].DeclaringType);
            Assert.Equal(typeof(TestObjectY), contract.Properties["y"].DeclaringType);
            Assert.True(contract.Properties["x"].Readable);
            Assert.True(contract.Properties["y"].Readable);
        }

        [Fact]
        public void MultipleBaseInternalPropertiesConfiguredCorrectly()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectX).GetProperty(nameof(TestObjectX.PropertyX), BindingFlags.Instance | BindingFlags.NonPublic), "propX", false);
            resolver.AddProperty(typeof(TestObjectX).GetProperty(nameof(TestObjectX.PropertyX2), BindingFlags.Instance | BindingFlags.NonPublic), "propX2", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectY)));

            Assert.Equal(2, contract.Properties.Count);
            Assert.Equal(nameof(TestObjectX.PropertyX), contract.Properties["propX"].UnderlyingName);
            Assert.Equal(nameof(TestObjectX.PropertyX2), contract.Properties["propX2"].UnderlyingName);
        }

        [Fact]
        public void InternalPropertyWithPrivateSetIsWritableForDerivedClass()
        {
            var resolver = new FluentContractResolver();
            resolver.AddProperty(typeof(TestObjectX).GetProperty(nameof(TestObjectX.PropertyX2), BindingFlags.Instance | BindingFlags.NonPublic), "propX2", false);

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectY)));

            Assert.Equal(nameof(TestObjectX.PropertyX2), contract.Properties["propX2"].UnderlyingName);
            Assert.True(contract.Properties["propX2"].Writable);

            var obj = new TestObjectY();
            contract.Properties["propX2"].ValueProvider.SetValue(obj, 123);
            Assert.Equal(123, obj.PropertyX2);
        }

        [Fact]
        public void CustomDefaultConstructorSet()
        {
            var resolver = new FluentContractResolver();
            var factory = (Func<TestObjectZ>)(() => new TestObjectZ(1));
            resolver.SetFactory(factory);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            var extractedFactory = contract.DefaultCreator.Target.GetType()
                .GetField(nameof(factory))
                .GetValue(contract.DefaultCreator.Target);

            Assert.Same(factory, extractedFactory);
        }

        [Fact]
        public void CustomOverrideConstructorSet()
        {
            var resolver = new FluentContractResolver();
            var factory = (Func<object[], TestObjectZ>)(_ => new TestObjectZ((int)_[0]));
            resolver.SetFactory(factory);

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            var extractedFactory = contract.OverrideCreator.Target.GetType()
                .GetField(nameof(factory))
                .GetValue(contract.OverrideCreator.Target);

            Assert.Same(factory, extractedFactory);
        }

        #region Тестовые объекты

        private interface ITestObject
        {
            int Property1 { get; }
        }

        private interface ITestObject2
        {
            bool Property4 { get; }
        }

        private class TestObjectA : ITestObject
        {
            public virtual int Property1 { get; set; }

            public int? Property2 { get; private set; }

            public string Property3 { get; }
        }

        private class TestObjectA2 : TestObjectA
        {
            public override int Property1 { get; set; }
        }

        private class TestObjectA22 : TestObjectA2
        {
            public override int Property1 { get; set; }
        }

        private class TestObjectA3 : TestObjectA
        {
            public new int Property1 { get; set; }
        }

        private class TestObjectB : TestObjectA, ITestObject2
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

            internal int PropertyX2 { get; private set; }
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
