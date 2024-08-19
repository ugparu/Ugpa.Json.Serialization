
# Configuring

There're next classes to configure serialization:

|Class|Description|
|:-:|-|
|[`Configurator`](#starting-configuring)|Common class used for configuring. All configuring process starts with creating instance of this class.|
|[`TypeConfigurator`](#configuring-type)|This class is used to configure type serialization.|
|[`PropertyConfigurator`](#configuring-property)|This class is used to configure property.|
|[`Configuration`](#completing-configuration)|Final class, containing configuration. Implements `ISerialiationBinder` and `IContractResolver` interfaces.|

### Starting configuring

To start configuring, create an instance of `Configurator` class. You can do it by calling `Configurator.Create()` method. Now you can configure serialization process by calling required methods.

|Method|Description|
|:-:|-|
|`WithBaseSerializationBinder`|Set base `ISerializationBinder`, used for type name binding if type is not configured by `TypeConfigurator`. If not called, `DefaultSerializationBinder` will be used.|
|`DisallowNullValues`|Disallow `null` values in JSON. By default `null` values are allowed.|
|`Configure<T>`|Configure type `T` by calling configure delegate.|
|`Complete`|Creates instance of `Configuration`, which can be used as `ISerializationBinder` and/or `IContractResolver`|

### Configuring type

To configure type there's `TypeConfigurator<T>` class. You can't instantiate this class manually, but you can work with it when calling `Configurator.Configure<T>` method, so instance of `TypeConfigurator<T>` is passed as configure delegate parameter.

There're next methods you can use to configure type:

|Method|Description|
|:-:|-|
|`HasRequiredProperty<TProp>`|Configure property as required property by calling configure delegate.|
|`HasOptionalProperty<TProp>`|Configure property as optional property by calling configure delegate.|
|`IgnoreProperty<TProp>`|Add property to ignore list, so it will be ignored through serialization process.|
|`HasContractName`|Set contract name for type `T` which will be associated with it in JSON in `$type` field. |
|`ConstructWith`|Set default or override (depending on method overload) constructor delegate for type when serialize.|

### Configuring property

To configure property there's `PropertyConfigurator<T, TProp>` class. As in case of `TypeConfigurator<T>`, you also can't instantiate this class manually, but you can work with it when calling `TypeConfigurator<T>.HasRequiredProperty<T>` or `TypeConfigurator<T>.HasOptionalProperty<T>` method, so instance of `PropertyConfigurator<T, TProp>` is passed as configure delegate parameter.

> [!NOTE]
> When calling property configure method, the property access expression must be direct `MemberExpression` (e.g. `_ => _.Foo`). Also, property must be the own property of configured type. Configuring properties from derived types is intentionally limited now.

There're next methods you can use to configure property:

|Method|Description|
|:-:|-|
|`HasName`|Set JSON field name for property.|
|`HasSerializeCondition`|Set condition to serialize property.|

### Completing configuration

To complete configuring and get a working configuration you need to call `Configurator.Complete` method. This call creates an instance of `Configuration` class, which can be used as `ISerializationBinder` and/or `IContractResolver`.

## Examples

### Creating configuration

```csharp
// null values are allowed.
var conf1 = Configurator.Create();

// null values are not allowed.
var conf2 = Configurator.Create()
    .DisallowNullValues();

// with base serialization binder.
var conf3 = Configurator.Create()
    .WithBaseSerializationBinder(new DefaultSerializationBinder());

// mixed.
var conf4 = Configurator.Create()
    .DisallowNullValues()
    .WithBaseSerializationBinder(new DefaultSerializationBinder());
```

### Configuring types

#### Configuring creators

```csharp
public sealed class Employee
{
    public Employee(Department department)
    {
        ...
    }
}
...
config
    .Configure<Employee>(t => t
        .ConstructWith(() => new Employee(predefinedDepartment)));
```

```csharp
public sealed class Employee
{
    public Employee(string name, int age)
    {
        ...
    }
}

...

// parameters are getten from JSON properties `name` and `age`.
config
    .Configure<Employee>(cfg => cfg
        .ConstructWith(_ => new Employee(
            (string)_[0], // name.
            (int)_[1]))); // age.
```

#### Configuring derived classes

```csharp
public abstract class Animal { }
public sealed class Dog : Animal { }
public sealed class Cat : Animal { }

...

configurator
    .Configure<Dog>(t => t.HasContractName("dog"))
    .Configure<Cat>(t => t.HasContractName("cat"));
```

### Configuring properties

#### Configuring property name

```csharp
public sealed class Employee
{
    public string FirstName { get; init; }    
    public string LastName { get; init; }
    public int Age { get; init; }
}
...
config
    .Configure<Employee>(t => t
        .HasRequiredProperty(_ => _.FirstName, p => p.HasName("name"))
        .HasRequiredProperty(_ => _.LastName, p => p.HasName("lastName")
        .HasOptionalProperty(_ => _.Age, p => p.HasName("age"));
```

#### Skipping property

```csharp
public sealed class Employee
{
    public string FirstName { get; init; }    
    public string LastName { get; init; }
    public int Age { get; init; }
}
...
config
    .Configure<Employee>(t => t
        .IgnoreProperty(_ => _.Age));
```

### Complete configuration sample
```csharp
public sealed class Emplyee
{
    public Emplyee(Department department)
    {
        Department = department;
    }
    
    public Department Department { get; }
    
    public string FirstName { get; init; }
    
    public int? Age { get; init; }
}
...
config
    .Configure<Emplyee>(t => t
        .HasRequiredProperty(_ => _.FirstName, p => p.HasName("name"))
        .HasOptionalProperty(_ => _.Age, p => p.HasName("age"))
        .ConstructWith(() => new Emplyee(department))
        .HasContractName("employee"));
```

### Applying configuration

```csharp

var config = Configurator
    .Create()
    ...
    .Complete();

var settings = new JsonSerializerSettings
{
    ContractResolver = config,
    SerializationBinder = config
};

var serializer = JsonSerializer.Create(settings);
```
