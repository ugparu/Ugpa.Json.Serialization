# Ugpa.Json.Serialization

This repository contains a simple implementation of fluent configurator for [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) serializer.

[![Nuget](https://img.shields.io/nuget/v/Ugpa.Json.Serialization?label=Ugpa.Json.Serialization&style=flat-square)](https://www.nuget.org/packages/Ugpa.Json.Serialization)
![build](https://img.shields.io/github/actions/workflow/status/ugparu/Ugpa.Json.Serialization/build.yml?branch=develop&label=Build%20and%20test&style=flat-square)
![publish](https://img.shields.io/github/actions/workflow/status/ugparu/Ugpa.Json.Serialization/publish.yml?event=release&label=Publish&style=flat-square)

With this library you can easily configure serializer like this:

```csharp
var config = Configurator
    .Create()
    .DisallowNullValues()
    .Configure<Animal>(t => t
        .HasRequiredProperty(_ => _.Age, p => p.HasName("age")))
    .Configure<Cat>(t => t
        .HasContractName("cat")
        .HasRequiredProperty(_ => _.Name, p => p.HasName("name"))
        .HasOptionalProperty(_ => _.CatOwner, p => p.HasName("owner").HasSerializeCondition(c => c.CatOwner is not null)))
    .Complete();
```

This kind of configuration provides you next benfits:
- [x] No attributes. Configuring without attributes allows you to create configurations even for casses you are not owning.
- [x] Variaty. You can use different configuration for the same types, e.g., for re-coding JSON formats using single DTO model.
- [x] Localized configuration. The entire configuration can be described in one place.
- [x] Extensibility. If you are using any IoC container and serialized classes are distributed by separate assemblies, you can split configuration on parts and place them next on serialized classes, and then use IoC to collect those parts in solid configuration.
- [x] Flexibility for generic types. If you are working with generic type, there's no need to create empty derived types for different generic subclasses, because you can create configuration for generics.

> [!IMPORTANT]
> Configurator is based on `IContractResolver` and `ISerializationBinder` interfaces implementation, so it's incompatible with any other resolvers and binders.

## Documentation

- [Configuring](doc/Configuring.md)
- [Release notes](doc/ReleaseNotes.md)
