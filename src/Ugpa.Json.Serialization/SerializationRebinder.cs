using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Ugpa.Json.Serialization;

internal sealed class SerializationRebinder(ISerializationBinder baseBinder) : ISerializationBinder
{
    private readonly Dictionary<Type, string> nameBindings = new();
    private readonly Dictionary<string, Type> typeBindings = new();

    public void AddBinding(Type type, string typeName)
    {
        nameBindings.Add(type, typeName);
        typeBindings.Add(typeName, type);
    }

    void ISerializationBinder.BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        if (nameBindings.TryGetValue(serializedType, out typeName))
        {
            assemblyName = null;
        }
        else
        {
            baseBinder.BindToName(serializedType, out assemblyName, out typeName);
        }
    }

    Type ISerializationBinder.BindToType(string? assemblyName, string typeName)
    {
        if (typeBindings.TryGetValue(typeName, out var type))
        {
            return type;
        }
        else
        {
            return baseBinder.BindToType(assemblyName, typeName);
        }
    }
}
