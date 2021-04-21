using Newtonsoft.Json;

namespace Ugpa.Json.Serialization
{
    public static class FluentContextExtensions
    {
        public static void Apply(this FluentContext context, JsonSerializerSettings settings)
        {
            settings.ContractResolver = context;
            settings.SerializationBinder = context;
            settings.TypeNameHandling = TypeNameHandling.All;
        }
    }
}
