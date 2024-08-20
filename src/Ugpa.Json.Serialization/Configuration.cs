using System;
using Newtonsoft.Json.Serialization;

namespace Ugpa.Json.Serialization;

/// <summary>
/// Represents a configuration of serializer.
/// </summary>
public sealed class Configuration : IContractResolver, ISerializationBinder
{
    private readonly IContractResolver resolver;
    private readonly ISerializationBinder binder;

    internal Configuration(IContractResolver resolver, ISerializationBinder binder)
    {
        this.resolver = resolver;
        this.binder = binder;
    }

    /// <inheritdoc/>
    void ISerializationBinder.BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        => binder.BindToName(serializedType, out assemblyName, out typeName);

    /// <inheritdoc/>
    Type ISerializationBinder.BindToType(string? assemblyName, string typeName)
        => binder.BindToType(assemblyName, typeName);

    /// <inheritdoc/>
    JsonContract IContractResolver.ResolveContract(Type type)
        => resolver.ResolveContract(type);
}
