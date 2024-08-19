using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Ugpa.Json.Serialization.Tests
{
    public sealed class ContractResolverTest
    {
        [Fact]
        public void DefaultContractIsValid()
        {
            IContractResolver resolver = Configurator.Create().Complete();

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
            Configurator
                .Create()
                .Configure<TestObjectA>(t =>
                    {
                        t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"));
                        Assert.Throws<ArgumentException>(() => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")));
                    })
                .Complete();
        }

        [Fact]
        public void DuplicatePropertySkipConfigurationSuccess()
        {
            Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .IgnoreProperty(_ => _.Property1)
                    .IgnoreProperty(_ => _.Property1));
        }

        [Fact]
        public void ErrorOnPropertyMultipleConfigurations()
        {
            Configurator
                .Create()
                .Configure<TestObjectA>(t =>
                {
                    t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA1"));
                    Assert.Throws<ArgumentException>(() => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA2")));
                });
        }

        [Fact]
        public void ErrorOnMultiplePropertyConfigurationsWithSameName()
        {
            Configurator
                .Create()
                .Configure<TestObjectA>(t =>
                {
                    t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"));
                    Assert.Throws<ArgumentException>(() => t.HasOptionalProperty(_ => _.Property2, p => p.HasName("propA")));
                });
        }

        [Fact]
        public void ErrorOnConfiguringBasePropertyFromDerivedType()
        {
            var configurator = Configurator.Create();
            Assert.Throws<InvalidOperationException>(
                () => configurator.Configure<TestObjectB>(t => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))));
        }

        [Fact]
        public void ErrorOnPropertyNameConflictConfiguration()
        {
            Configurator
                .Create()
                .Configure<TestObjectA>(t =>
                {
                    t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"));
                    Assert.Throws<ArgumentException>(() => t.HasOptionalProperty(_ => _.Property2, p => p.HasName("propA")));
                });
        }

        [Fact]
        public void ConfiguredPropertyNameIsValid()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))
                    .HasOptionalProperty(_ => _.Property2, p => p.HasName("propB"))
                    .HasOptionalProperty(_ => _.Property3, p => p.HasName("propC")))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(nameof(TestObjectA.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property2), contract.Properties["propB"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property3), contract.Properties["propC"].UnderlyingName);
        }

        [Fact]
        public void PropertyRequiredAllowNullIsValid()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))
                    .HasRequiredProperty(_ => _.Property2, p => p.HasName("propB")))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(Required.Default, contract.Properties["propA"].Required);
            Assert.Equal(Required.Always, contract.Properties["propB"].Required);
        }

        [Fact]
        public void PropertyRequiredDisallowNullIsValid()
        {
            IContractResolver resolver = Configurator
                .Create()
                .DisallowNullValues()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))
                    .HasRequiredProperty(_ => _.Property2, p => p.HasName("propB")))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectA));

            Assert.Equal(Required.DisallowNull, contract.Properties["propA"].Required);
            Assert.Equal(Required.Always, contract.Properties["propB"].Required);
        }

        [Fact]
        public void ConfiguredPropertiesInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Configure<TestObjectB>(t => t.HasOptionalProperty(_ => _.Property4, p => p.HasName("propD")))
                .Complete();

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
        public void ConfiguredSkippedPropertiesInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t.IgnoreProperty(_ => _.Property1))
                .Configure<TestObjectB>(t => t.IgnoreProperty(_ => _.Property4))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectB));

            Assert.Equal(3, contract.Properties.Count);
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
            Assert.False(contract.Properties.Contains(nameof(TestObjectB.Property4)));
        }

        [Fact]
        public void OverriddenPropertyConfigurationInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Complete();

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
        public void OverriddenSkippedPropertyConfigurationInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA2)));

            Assert.Equal(2, contract.Properties.Count);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA2.Property1)));
        }

        [Fact]
        public void OverriddenPropertyIntermediateConfigurationInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA2>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Complete();

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
        public void OverriddenSkippedPropertyIntermediateConfigurationInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA2>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA22)));

            Assert.Equal(2, contract.Properties.Count);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA22.Property1)));
        }

        [Fact]
        public void HiddenPropertyConfigurationInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA3)));
            Assert.Equal(4, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA3.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA3.Property1), contract.Properties[nameof(TestObjectA3.Property1)].UnderlyingName);

            Assert.Equal(typeof(TestObjectA), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA3), contract.Properties[nameof(TestObjectA3.Property1)].DeclaringType);
        }

        [Fact]
        public void HiddenPropertyOverlapsSkippedProperty()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA3>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA3)));
            Assert.Equal(3, contract.Properties.Count);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA3.Property1)].DeclaringType);
        }

        [Fact]
        public void ConfiguredInterfacePropertiesInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<ITestObject>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property3, p => p.HasName("propC")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Equal(3, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectA.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property2), contract.Properties[nameof(TestObjectA.Property2)].UnderlyingName);
            Assert.Equal(nameof(TestObjectA.Property3), contract.Properties["propC"].UnderlyingName);

            Assert.Equal(typeof(TestObjectA), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties[nameof(TestObjectA.Property2)].DeclaringType);
            Assert.Equal(typeof(TestObjectA), contract.Properties["propC"].DeclaringType);

            var json = JsonConvert.SerializeObject(new TestObjectA(), new JsonSerializerSettings { ContractResolver = resolver });
        }

        [Fact]
        public void ExplicitInterfaceConfiguration()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<ITestInterface3>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))
                    .HasOptionalProperty(_ => _.Property2, p => p.HasName("propB")))
                .Configure<TestObjectWithExplicitInterfaceImpl>(t => t
                    .HasOptionalProperty(_ => _.Property0, p => p.HasName("prop0")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectWithExplicitInterfaceImpl)));

            Assert.Equal(3, contract.Properties.Count);

            Assert.Equal(nameof(TestObjectWithExplicitInterfaceImpl.Property0), contract.Properties["prop0"].UnderlyingName);
            Assert.Equal(nameof(ITestInterface3.Property1), contract.Properties["propA"].UnderlyingName);
            Assert.Equal(nameof(ITestInterface3.Property2), contract.Properties["propB"].UnderlyingName);

            Assert.Equal(typeof(TestObjectWithExplicitInterfaceImpl), contract.Properties["prop0"].DeclaringType);
            Assert.Equal(typeof(ITestInterface3), contract.Properties["propA"].DeclaringType);
            Assert.Equal(typeof(ITestInterface3), contract.Properties["propB"].DeclaringType);

            Assert.False(contract.Properties["propA"].Writable);
            Assert.True(contract.Properties["propB"].Writable);

            var obj = JsonConvert.DeserializeObject<TestObjectWithExplicitInterfaceImpl>(
                "{'prop0':321,'propA':567,'propB':951}",
                new JsonSerializerSettings { ContractResolver = resolver });

            Assert.Equal(321, obj.Property0);
            Assert.Equal(951, ((ITestInterface3)obj).Property2);
        }

        [Fact]
        public void ConfiguredInterfaceSkippedPropertiesInherits()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<ITestObject>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Configure<TestObjectA>(t => t
                    .IgnoreProperty(_ => _.Property3))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Single(contract.Properties);

            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property3)));
        }

        [Fact]
        public void MultipleInterfacesPropertiesResolverCorrectly()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<ITestObject>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("prop1")))
                .Configure<ITestObject2>(t => t
                    .HasOptionalProperty(_ => _.Property4, p => p.HasName("prop4")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectB)));

            Assert.Equal(5, contract.Properties.Count);
            Assert.Equal(nameof(ITestObject.Property1), contract.Properties["prop1"].UnderlyingName);
            Assert.Equal(nameof(ITestObject2.Property4), contract.Properties["prop4"].UnderlyingName);
        }

        [Fact]
        public void OverriddenPropertyConfigurationOverlap()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Configure<TestObjectA2>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA2")))
                .Complete();

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
        public void OverriddenSkippedPropertyConfigurationOverlap()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Configure<TestObjectA2>(t => t
                    .IgnoreProperty(_ => _.Property1))
                .Complete();

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
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Configure<TestObjectA3>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA2")))
                .Complete();

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
            IContractResolver resolver = Configurator
                .Create()
                .Configure<ITestObject>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA1")))
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA2")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));
            Assert.Equal(3, contract.Properties.Count);
            Assert.Equal(nameof(ITestObject.Property1), contract.Properties["propA2"].UnderlyingName);
            Assert.Equal(typeof(TestObjectA), contract.Properties["propA2"].DeclaringType);
        }

        [Fact]
        public void SkippedPropertyPriorityOverConfiguration()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA1"))
                    .IgnoreProperty(_ => _.Property1))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectA)));

            Assert.Equal(2, contract.Properties.Count);
            Assert.False(contract.Properties.Contains(nameof(TestObjectA.Property1)));
        }

        [Fact]
        public void TypesWithSameNamePropertiesConfiguresSuccessfully()
        {
            var resolver = Configurator
                .Create()
                .Configure<TestObjectA>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")))
                .Configure<TestObjectC>(t => t
                    .HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")));
        }

        [Fact]
        public void ErrorOnDerivedClassPropertyNameConflict()
        {
            var configurator1 = Configurator
                .Create()
                .Configure<TestObjectA>(t => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA")));

            var configurator2 = Configurator
                .Create()
                .Configure<TestObjectB>(t => t.HasOptionalProperty(_ => _.Property4, p => p.HasName("propA")));

            Assert.Throws<ArgumentException>(
                () => configurator1.Configure<TestObjectB>(t => t.HasOptionalProperty(_ => _.Property4, p => p.HasName("propA"))));

            Assert.Throws<ArgumentException>(
                () => configurator2.Configure<TestObjectA>(t => t.HasOptionalProperty(_ => _.Property1, p => p.HasName("propA"))));
        }

        [Fact]
        public void InternalPropertiesConfiguredSuccessfully()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectX>(t => t
                    .HasOptionalProperty(_ => _.PropertyX, p => p.HasName("x")))
                .Configure<TestObjectY>(t => t
                    .HasOptionalProperty(_ => _.PropertyY, p => p.HasName("y")))
                .Complete();

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
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectX>(t => t
                    .HasOptionalProperty(_ => _.PropertyX, p => p.HasName("propX"))
                    .HasOptionalProperty(_ => _.PropertyX2, p => p.HasName("propX2")))
                .Complete();

            var contract = Assert.IsType<JsonObjectContract>(resolver.ResolveContract(typeof(TestObjectY)));

            Assert.Equal(2, contract.Properties.Count);
            Assert.Equal(nameof(TestObjectX.PropertyX), contract.Properties["propX"].UnderlyingName);
            Assert.Equal(nameof(TestObjectX.PropertyX2), contract.Properties["propX2"].UnderlyingName);
        }

        [Fact]
        public void InternalPropertyWithPrivateSetIsWritableForDerivedClass()
        {
            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectX>(t => t
                    .HasOptionalProperty(_ => _.PropertyX2, p => p.HasName("propX2")))
                .Complete();

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
            Func<TestObjectZ> factory = () => new TestObjectZ(1);

            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectZ>(t => t
                    .ConstructWith(factory))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            var extractedFactory = contract.DefaultCreator.Target.GetType()
                .GetField(nameof(factory))
                .GetValue(contract.DefaultCreator.Target);

            Assert.Same(factory, extractedFactory);
        }

        [Fact]
        public void CustomOverrideConstructorSet()
        {
            Func<object[], TestObjectZ> factory = _ => new TestObjectZ((int)_[0]);

            IContractResolver resolver = Configurator
                .Create()
                .Configure<TestObjectZ>(t => t
                    .ConstructWith(factory))
                .Complete();

            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(TestObjectZ));

            var extractedFactory = contract.OverrideCreator.Target.GetType()
                .GetField(nameof(factory))
                .GetValue(contract.OverrideCreator.Target);

            Assert.Same(factory, extractedFactory);
        }

        #region Test objects

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

            public string Property3 => "FooBar";
        }

        private interface ITestInterface3
        {
            int Property1 { get; }

            int Property2 { get; set; }
        }

        private sealed class TestObjectWithExplicitInterfaceImpl : ITestInterface3
        {
            public int Property0 { get; set; }

            int ITestInterface3.Property1 => throw new NotImplementedException();

            int ITestInterface3.Property2 { get; set; }
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
