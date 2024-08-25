using System;
using System.Linq.Expressions;

namespace Ugpa.Json.Serialization;

/// <summary>
/// Provides functionality to configure type <typeparamref name="T"/> serializing.
/// </summary>
/// <typeparam name="T">Configured type.</typeparam>
public sealed class TypeConfigurator<T>
{
    private readonly ITypeConfigurator configurator;

    internal TypeConfigurator(ITypeConfigurator configurator)
    {
        this.configurator = configurator;
    }

    /// <summary>
    /// Configures required property.
    /// </summary>
    /// <typeparam name="TProp">Property type.</typeparam>
    /// <param name="property">Property access expression.</param>
    /// <param name="configure">Action to configure property.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> HasRequiredProperty<TProp>(Expression<Func<T, TProp>> property, Action<PropertyConfigurator<T, TProp>> configure)
        => HasProperty(property, configure, true);

    /// <summary>
    /// Configures required property.
    /// </summary>
    /// <typeparam name="TProp">Property type.</typeparam>
    /// <param name="property">Property access expression.</param>
    /// <param name="configure">Action to configure property.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> HasOptionalProperty<TProp>(Expression<Func<T, TProp>> property, Action<PropertyConfigurator<T, TProp>> configure)
        => HasProperty(property, configure, false);

    /// <summary>
    /// Configure property to be ignored when serializing.
    /// </summary>
    /// <typeparam name="TProp">Property type.</typeparam>
    /// <param name="property">Property access expression.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> IgnoreProperty<TProp>(Expression<Func<T, TProp>> property)
    {
        configurator.IgnoreProperty(property);
        return this;
    }

    /// <summary>
    /// Configures a string, representing type <typeparamref name="T"/> in json.
    /// </summary>
    /// <param name="name">A string, representing type <typeparamref name="T"/>.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> HasContractName(string name)
    {
        configurator.SetContractName<T>(name);
        return this;
    }

    /// <summary>
    /// Configures a type <typeparamref name="T"/> parameterless factory when deserializing.
    /// </summary>
    /// <param name="factory">A func, creating instance of <typeparamref name="T"/>.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> ConstructWith(Func<T> factory)
    {
        configurator.SetDefaultCreator(factory);
        return this;
    }

    /// <summary>
    /// Configures a type <typeparamref name="T"/> factory when deserializing.
    /// </summary>
    /// <param name="factory">A func, creating instance of <typeparamref name="T"/>.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> ConstructWith(Func<object[], T> factory)
    {
        configurator.SetOverrideCreator(factory);
        return this;
    }

    /// <summary>
    /// Configures a type <typeparamref name="T"/> factory when deserializing.
    /// </summary>
    /// <typeparam name="TFunc">Type of factory delegate.</typeparam>
    /// <param name="factory">A func, creating instance of <typeparamref name="T"/>.</param>
    /// <returns>Instance of <see cref="TypeConfigurator{T}"/>.</returns>
    public TypeConfigurator<T> ConstructWith<TFunc>(Expression<TFunc> factory)
        where TFunc : Delegate
    {
        configurator.SetOverrideCreator<T, TFunc>(factory);
        return this;
    }

    private TypeConfigurator<T> HasProperty<TProp>(Expression<Func<T, TProp>> property, Action<PropertyConfigurator<T, TProp>> configure, bool isRequired)
    {
        var config = new PropertyConfigurator();
        var prop = new PropertyConfigurator<T, TProp>(config);
        configure(prop);
        configurator.AddProperty(property, config.Name, isRequired, config.SerializeCondition);
        return this;
    }

    private sealed class PropertyConfigurator : IPropertyConfigurator<T>
    {
        public string? Name { get; private set; }

        public Func<T, bool>? SerializeCondition { get; private set; }

        public void SetName(string name)
            => Name = name;

        public void SetSerializeCondition(Func<T, bool>? serializeCondition)
            => SerializeCondition = serializeCondition;
    }
}
