using System;

namespace Ugpa.Json.Serialization;

/// <summary>
/// Provides functionality to configure property.
/// </summary>
/// <typeparam name="T">Property owner type.</typeparam>
/// <typeparam name="TProp">Property type.</typeparam>
public sealed class PropertyConfigurator<T, TProp>
{
    private readonly IPropertyConfigurator<T> configurator;

    internal PropertyConfigurator(IPropertyConfigurator<T> configurator)
    {
        this.configurator = configurator;
    }

    /// <summary>
    /// Configures property name in json.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <returns>Instance of <see cref="PropertyConfigurator{T, TProp}"/>.</returns>
    public PropertyConfigurator<T, TProp> HasName(string name)
    {
        configurator.SetName(name);
        return this;
    }

    /// <summary>
    /// Configures property serialize condition.
    /// </summary>
    /// <param name="condition">Serialize condition.</param>
    /// <returns>Instance of <see cref="PropertyConfigurator{T, TProp}"/>.</returns>
    public PropertyConfigurator<T, TProp> HasSerializeCondition(Func<T, bool> condition)
    {
        configurator.SetSerializeCondition(condition);
        return this;
    }
}
