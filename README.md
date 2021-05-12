# Ugpa.Json.Serialization
This repository contains a simple implementation of fluent configurator for [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) serializer.
## Creating configurator
```csharp
var context1 = new FluentContext(); // null values are allowed.
var context2 = new FluentContext(true); // null values are allowed.
var context3 = new FluentContext(false); // null values are not allowed.
```
## Configuring
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
context.Configure<Employee>(cfg => cfg.ConstructWith(_ => new Employee((string)_[0], (int)_[1]))); // parameters are getten from JSON properties `name` and `age`.
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
