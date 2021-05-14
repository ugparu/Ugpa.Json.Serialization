using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Ugpa.Json.Serialization
{
    internal sealed class FluentSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<Type, string> nameBindings = new();
        private readonly Dictionary<string, Type> typeBindings = new();

        public void AddBinding(Type type, string typeName)
        {
            nameBindings.Add(type, typeName);
            typeBindings.Add(typeName, type);
        }

        public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (nameBindings.TryGetValue(serializedType, out typeName))
            {
                assemblyName = null;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public override Type BindToType(string? assemblyName, string typeName)
        {
            if (typeBindings.TryGetValue(typeName, out var type))
            {
                return type;
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
