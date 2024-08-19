using System;
using System.Linq.Expressions;

namespace Ugpa.Json.Serialization;

internal interface ITypeConfigurator
{
    void SetContractName<T>(string name);

    void SetDefaultCreator<T>(Func<T> factory);

    void SetOverrideCreator<T>(Func<object[], T> factory);

    void IgnoreProperty<T, TProp>(Expression<Func<T, TProp>> property);

    void AddProperty<T, TProp>(Expression<Func<T, TProp>> property, string? name, bool isRequired, Func<T, bool>? serializeCondition);
}
