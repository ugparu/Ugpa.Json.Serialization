# Ugpa.Json.Serialization
This repository contains a simple implementation of fluent configurator for [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) serializer.

![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/ugparu/Ugpa.Json.Serialization/build%20and%20test/develop?label=develop)
[![Nuget](https://img.shields.io/nuget/v/Ugpa.Json.Serialization)](https://www.nuget.org/packages/Ugpa.Json.Serialization)

## Important notes
- Configurator is based on `IContractResolver` and `ISerializationBinder` interfaces implementation, so it's incompatible with any other resolvers and binders.

## Configuring

### Creating configurator
```csharp
var context1 = new FluentContext(); // null values are allowed.
var context2 = new FluentContext(true); // null values are allowed.
var context3 = new FluentContext(false); // null values are not allowed.
```

### Configuring properties
You can map class properties to JSON property names. Also you can define is property required or optional.
```csharp
public sealed class Employee
{
    public string FirstName { get; init; }    
    public string LastName { get; init; }
    public int Age { get; init; }
}
...
context.Configure<Employee>(cfg => cfg
    .HasProperty(_ => _.FirstName, "name") // required property.
    .HasProperty(_ => _.LastName, "lastName", false) // optional property.
    .HasProperty(_ => _.Age, "age", true)); // required property (explicitly).
```

### Skipping property
If there's no need to serialize/deserialize property, you can configure it to be skipped.
```csharp
public sealed class Employee
{
    public string FirstName { get; init; }    
    public string LastName { get; init; }
    public int Age { get; init; }
}
...
context.Configure<Employee>(cfg => cfg
    .IgnoreProperty(_ => _.Age)); // no need to serialize/deserialize.
```

### Type name binding
You can bind class name to JSON `$type` property.
```csharp
public abstract class Animal { }
public sealed class Dog : Animal { }
public sealed class Cat : Animal { }
...
context
    .Configure<Dog>(cfg => cfg.HasContractName("dog"))
    .Configure<Cat>(cfg => cfg.HasContractName("cat"));
```

### Configuring creators
You can declare custom creator for class.
For predefined constructor parameters use next:
```csharp
public sealed class Employee
{
    public Employee(Department department)
    {
        ...
    }
}
...
context.Configure<Employee>(cfg => cfg.ConstructWith(() => new Employee(predefinedDepartment)));
```
For constructors with parameters, which should be getten from JSON, use next:
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
context.Configure<Employee>(cfg => cfg
    .ConstructWith(_ => new Employee(
        (string)_[0], // name.
        (int)_[1]))); // age.
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
context.Configure<Emplyee>(cfg => cfg
    .HasProperty(_ => _.FirstName, "name")
    .HasProperty(_ => _.Age, "age", false)
    .ConstructWith(() => new Emplyee(department))
    .HasContractName("employee"));
```

### Applying configuration
```csharp
var settings = new JsonSerializerSettings
{
    ContractResolver = context,
    SerializationBinder = context
};

var serializer = JsonSerializer.Create(settings);
```
