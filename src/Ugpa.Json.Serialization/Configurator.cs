using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Ugpa.Json.Serialization.Properties;
using MemberData = (string Name, bool IsRequired, System.Func<object, bool>? SerializeCondition);

namespace Ugpa.Json.Serialization;

/// <summary>
/// Provides functionality to configure serializer.
/// </summary>
public sealed class Configurator : ITypeConfigurator
{
    private readonly HashSet<Type> configuredTypes = new();
    private readonly HashSet<(Type Type, string ContractName)> bindings = new();

    private readonly Dictionary<Type, Dictionary<MemberInfo, MemberData>> properties = new();
    private readonly Dictionary<Type, HashSet<MemberInfo>> ignored = new();
    private readonly Dictionary<Type, Func<object>> defaultCreators = new();
    private readonly Dictionary<Type, ObjectConstructor<object>> overrideCreators = new();

    private ISerializationBinder? baseBinder;
    private bool allowNullValues = true;

    internal Configurator()
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Configurator"/>.
    /// </summary>
    /// <returns>New instance of <see cref="Configurator"/>.</returns>
    public static Configurator Create()
        => new Configurator();

    /// <summary>
    /// Set base <see cref="ISerializationBinder"/>, used for not configured types.
    /// </summary>
    /// <param name="binder">Base binder.</param>
    /// <returns>Instance of <see cref="Configurator"/>.</returns>
    public Configurator WithBaseSerializationBinder(ISerializationBinder binder)
    {
        baseBinder = binder;
        return this;
    }

    /// <summary>
    /// Disallow null values.
    /// </summary>
    /// <returns>Instance of <see cref="Configurator"/>.</returns>
    public Configurator DisallowNullValues()
    {
        allowNullValues = false;
        return this;
    }

    /// <summary>
    /// Configuring serialization fot <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Configured type.</typeparam>
    /// <param name="configure">Configure delegate.</param>
    /// <returns>Instance of <see cref="Configurator"/>.</returns>
    public Configurator Configure<T>(Action<TypeConfigurator<T>> configure)
    {
        if (configuredTypes.Contains(typeof(T)))
        {
            throw new InvalidOperationException();
        }

        configure(new TypeConfigurator<T>(this));

        configuredTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Creates a configured instance of <see cref="Configuration"/>.
    /// </summary>
    /// <returns>A configured instance of <see cref="Configuration"/>.</returns>
    public Configuration Complete()
    {
        var binder = new SerializationRebinder(baseBinder ?? new DefaultSerializationBinder());
        var resolver = new ContractResolver()
        {
            AllowNullValues = allowNullValues,
        };

        foreach (var binding in bindings)
        {
            binder.AddBinding(binding.Type, binding.ContractName);
        }

        foreach (var defaultCreator in defaultCreators)
        {
            resolver.SetDefaultCreator(defaultCreator.Key, defaultCreator.Value);
        }

        foreach (var overrideCreator in overrideCreators)
        {
            resolver.SetOverrideCreator(overrideCreator.Key, overrideCreator.Value);
        }

        foreach (var ignoredProperty in ignored)
        {
            foreach (var member in ignoredProperty.Value)
            {
                resolver.SkipProperty(ignoredProperty.Key, member);
            }
        }

        foreach (var configuredProperty in properties)
        {
            foreach (var data in configuredProperty.Value)
            {
                resolver.AddPropertyInfo(configuredProperty.Key, data.Key, data.Value.Name, data.Value.IsRequired, data.Value.SerializeCondition);
            }
        }

        return new Configuration(resolver, binder);
    }

    void ITypeConfigurator.SetContractName<T>(string name)
        => bindings.Add((typeof(T), name));

    void ITypeConfigurator.SetDefaultCreator<T>(Func<T> factory)
        => defaultCreators[typeof(T)] = () => factory()!;

    void ITypeConfigurator.SetOverrideCreator<T>(Func<object[], T> factory)
        => overrideCreators[typeof(T)] = _ => factory(_)!;

    void ITypeConfigurator.IgnoreProperty<T, TProp>(Expression<Func<T, TProp>> property)
    {
        var member = ReflectionUtils.GetMemberInfo(property);
        if (!ignored.TryGetValue(member.DeclaringType, out var typeInfo))
        {
            typeInfo = new();
            ignored[member.DeclaringType] = typeInfo;
        }

        typeInfo.Add(member);
    }

    void ITypeConfigurator.AddProperty<T, TProp>(Expression<Func<T, TProp>> property, string? name, bool isRequired, Func<T, bool>? serializeCondition)
    {
        var member = ReflectionUtils.GetMemberInfo(property);
        var data = (
            Name: name ?? member.Name,
            isRequired,
            SerializeCondition: serializeCondition is null ? null : (Func<object, bool>)(o => serializeCondition((T)o)));

        var inheritanceProperties = properties
            .Where(_ =>
                _.Key != member.DeclaringType &&
                (_.Key.IsAssignableFrom(member.DeclaringType) || member.DeclaringType.IsAssignableFrom(_.Key)))
            .SelectMany(_ => _.Value)
            .Where(_ => _.Value.Name == data.Name)
            .ToArray();

        if (inheritanceProperties.Any())
        {
            throw new ArgumentException(string.Format(
                Resources.FluentContractResolver_InheritancePropertyNameConflict,
                member.DeclaringType,
                data.Name,
                inheritanceProperties[0].Key.DeclaringType,
                inheritanceProperties[0].Key.Name));
        }

        if (!properties.TryGetValue(member.DeclaringType, out var typeInfo))
        {
            typeInfo = new();
            properties[member.DeclaringType] = typeInfo;
        }

        var propertyInfo = typeInfo.Where(_ => _.Value.Name == data.Name).ToArray();
        if (propertyInfo.Any())
        {
            throw new ArgumentException(string.Format(
                Resources.FluentContractResolver_PropertyNameConflict,
                member.DeclaringType,
                data.Name,
                propertyInfo[0].Key.Name));
        }

        typeInfo.Add(member, data);
    }
}
