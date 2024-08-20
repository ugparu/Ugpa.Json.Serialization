using Newtonsoft.Json.Serialization;
using Xunit;

namespace Ugpa.Json.Serialization.Tests
{
    public sealed class ConfiguratorTest
    {
        [Fact]
        public void TypeNameBindingIsValid()
        {
            ISerializationBinder binder = Configurator
                .Create()
                .Configure<ConfiguratorTest>(_ => _.HasContractName("test"))
                .Complete();

            binder.BindToName(typeof(ConfiguratorTest), out _, out var name);
            Assert.Equal("test", name);

            var type = binder.BindToType(null, "test");
            Assert.Equal(typeof(ConfiguratorTest), type);
        }
    }
}
