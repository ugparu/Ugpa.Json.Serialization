using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Ugpa.Json.Serialization
{
    /// <summary>
    /// Represents a fluent style configurator.
    /// </summary>
    public sealed class FluentContext : IContractResolver, ISerializationBinder
    {
        private readonly HashSet<Type> configuredTypes = new();

        private readonly FluentContractResolver resolver = new();
        private readonly FluentSerializationBinder binder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentContext"/> class.
        /// </summary>
        public FluentContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentContext"/> class.
        /// </summary>
        /// <param name="allowNullValues">Define if null values are allowed.</param>
        public FluentContext(bool allowNullValues)
        {
            resolver.AllowNullValues = allowNullValues;
        }

        /// <summary>
        /// Configures mapping for specified type.
        /// </summary>
        /// <typeparam name="T">Type that should be configured.</typeparam>
        /// <param name="builder">Configure delegate.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContext Configure<T>(Action<FluentContractBuilder<T>> builder)
            where T : class
        {
            if (configuredTypes.Contains(typeof(T)))
            {
                throw new InvalidOperationException();
            }

            builder(new FluentContractBuilder<T>(resolver, binder));
            configuredTypes.Add(typeof(T));

            return this;
        }

        /// <inheritdoc/>
        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
            => ((ISerializationBinder)binder).BindToName(serializedType, out assemblyName, out typeName);

        /// <inheritdoc/>
        public Type BindToType(string? assemblyName, string typeName)
            => ((ISerializationBinder)binder).BindToType(assemblyName, typeName);

        /// <inheritdoc/>
        public JsonContract ResolveContract(Type type)
            => resolver.ResolveContract(type);
    }
}
