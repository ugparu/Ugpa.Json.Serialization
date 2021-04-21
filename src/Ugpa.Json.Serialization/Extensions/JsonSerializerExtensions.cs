using System.IO;

namespace Newtonsoft.Json.Serialization
{
    public static class JsonSerializerExtensions
    {
        public static T Deserialize<T>(this JsonSerializer serializer, TextReader reader)
            => (T)serializer.Deserialize(reader, typeof(T));
    }
}
