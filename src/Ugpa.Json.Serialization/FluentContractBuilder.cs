using System;
using System.Linq.Expressions;
using Ugpa.Json.Serialization.Properties;

namespace Ugpa.Json.Serialization
{
    public sealed class FluentContractBuilder<T>
    {
        private readonly FluentContractResolver resolver;
        private readonly FluentSerializationBinder binder;

        internal FluentContractBuilder(FluentContractResolver resolver, FluentSerializationBinder binder)
        {
            this.resolver = resolver;
            this.binder = binder;
        }

        public FluentContractBuilder<T> HasProperty<TProp>(Expression<Func<T, TProp>> property, string name, bool isRequired = true)
        {
            if (!(property.Body is MemberExpression memberExpression))
                throw new ArgumentException(Resources.FluentContractBuilder_NotMemberExpression);

            resolver.AddProperty(memberExpression.Member, name, isRequired);

            return this;
        }

        public FluentContractBuilder<T> HasContractName(string name)
        {
            binder.AddBinding(typeof(T), name);
            return this;
        }
    }
}
