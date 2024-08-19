using System;
using System.Linq.Expressions;

namespace Ugpa.Json.Serialization
{
    /// <summary>
    /// Represents an fluent configurator for specified type.
    /// </summary>
    /// <typeparam name="T">Configured type.</typeparam>
    [Obsolete($"This class is obsolete. Use {nameof(TypeConfigurator<T>)} instead.")]
    public sealed class FluentContractBuilder<T>
    {
        private readonly ContractResolver resolver;
        private readonly SerializationRebinder binder;

        internal FluentContractBuilder(ContractResolver resolver, SerializationRebinder binder)
        {
            this.resolver = resolver;
            this.binder = binder;
        }

        /// <summary>
        /// Configures default factory.
        /// </summary>
        /// <param name="factory">Default factory delegate.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> ConstructWith(Func<T> factory)
        {
            resolver.SetDefaultCreator(typeof(T), () => factory()!);
            return this;
        }

        /// <summary>
        /// Configures parametrized factory.
        /// </summary>
        /// <param name="factory">Parametrized factory delegate.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> ConstructWith(Func<object[], T> factory)
        {
            resolver.SetOverrideCreator(typeof(T), _ => factory(_)!);
            return this;
        }

        /// <summary>
        /// Configures property.
        /// </summary>
        /// <typeparam name="TProp">Property type.</typeparam>
        /// <param name="property">Property access expression.</param>
        /// <param name="name">JSON property name.</param>
        /// <param name="isRequired"><see langword="true"/> if property is required or <see langword="false"/> if property is optional.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> HasProperty<TProp>(Expression<Func<T, TProp>> property, string name, bool isRequired = true)
        {
            var member = ReflectionUtils.GetMemberInfo(property);
            resolver.AddPropertyInfo(typeof(T), member, name, isRequired, null);
            return this;
        }

        /// <summary>
        /// Configures property to be ignored.
        /// </summary>
        /// <typeparam name="TProp">Property type.</typeparam>
        /// <param name="property">Property access expression.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> IgnoreProperty<TProp>(Expression<Func<T, TProp>> property)
        {
            var member = ReflectionUtils.GetMemberInfo(property);
            resolver.SkipProperty(typeof(T), member);
            return this;
        }

        /// <summary>
        /// Configures JSON type name.
        /// </summary>
        /// <param name="name">JSON type name.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> HasContractName(string name)
        {
            binder.AddBinding(typeof(T), name);
            return this;
        }
    }
}
