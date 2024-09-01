# 2.1.1
* Fixed deserializing of `Nullable` arguments in override creator expression when argument is `null`.

# 2.1.0
* Added `ConstructWith` method overload, accepting `Expression<T>` as parameter.

# 2.0.0
* Public API reworked. Classes `FluentContext` and `FluentContractBuilder` are obsolete. New classes was introduced for configuring. See more in [configuration guide](Configuring.md).
