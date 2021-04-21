using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Ugpa.Json.Serialization
{
    public sealed class FluentContext : IContractResolver, ISerializationBinder
    {
        private readonly HashSet<Type> configuredTypes = new HashSet<Type>();

        private readonly FluentContractResolver resolver = new FluentContractResolver();
        private readonly FluentSerializationBinder binder = new FluentSerializationBinder();

        public FluentContext()
        {
        }

        public FluentContext(bool allowNullValues)
        {
            resolver.AllowNullValues = allowNullValues;
        }

        public FluentContext Configure<T>(Action<FluentContractBuilder<T>> builder)
        {
            if (configuredTypes.Contains(typeof(T)))
                throw new InvalidOperationException();

            builder(new FluentContractBuilder<T>(resolver, binder));
            configuredTypes.Add(typeof(T));

            return this;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            => binder.BindToName(serializedType, out assemblyName, out typeName);

        public Type BindToType(string assemblyName, string typeName)
            => binder.BindToType(assemblyName, typeName);

        public JsonContract ResolveContract(Type type)
            => resolver.ResolveContract(type);
    }
}
