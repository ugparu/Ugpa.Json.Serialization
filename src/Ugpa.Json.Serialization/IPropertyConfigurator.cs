using System;

namespace Ugpa.Json.Serialization;

internal interface IPropertyConfigurator<T>
{
    void SetName(string name);

    void SetSerializeCondition(Func<T, bool> serializeCondition);
}
