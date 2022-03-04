using System;
using System.Linq.Expressions;
using Ugpa.Json.Serialization.Properties;

namespace Ugpa.Json.Serialization
{
    /// <summary>
    /// Represents an fluent configurator for specified type.
    /// </summary>
    /// <typeparam name="T">Configured type.</typeparam>
    public sealed class FluentContractBuilder<T>
    {
        private readonly FluentContractResolver resolver;
        private readonly FluentSerializationBinder binder;

        internal FluentContractBuilder(FluentContractResolver resolver, FluentSerializationBinder binder)
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
            resolver.SetFactory(factory);
            return this;
        }

        /// <summary>
        /// Configures parametrised factory.
        /// </summary>
        /// <param name="factory">Parametrised factory delegate.</param>
        /// <returns>This instance of configurator.</returns>
        public FluentContractBuilder<T> ConstructWith(Func<object[], T> factory)
        {
            resolver.SetFactory(factory);
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
            if (property.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(Resources.FluentContractBuilder_NotMemberExpression);
            }

            resolver.AddProperty(memberExpression.Member, name, isRequired);

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
