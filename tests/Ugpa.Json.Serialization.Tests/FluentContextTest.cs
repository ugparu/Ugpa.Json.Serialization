using Xunit;

namespace Ugpa.Json.Serialization.Tests
{
    public sealed class FluentContextTest
    {
        [Fact]
        public void TypeNameBindingIsValid()
        {
            var context = new FluentContext();
            context.Configure<FluentContextTest>(_ => _.HasContractName("test"));

            context.BindToName(typeof(FluentContextTest), out _, out var name);
            Assert.Equal("test", name);

            var type = context.BindToType(null, "test");
            Assert.Equal(typeof(FluentContextTest), type);
        }
    }
}
