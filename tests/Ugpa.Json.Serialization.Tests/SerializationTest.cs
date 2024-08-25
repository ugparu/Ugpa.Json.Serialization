using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Ugpa.Json.Serialization.Tests;

public sealed class SerializationTest
{
    [Fact]
    public void DeserializeConfiguredPropertyNamesTest()
    {
        var json = "{'a':5,'p':4}";

        var config = Configurator
            .Create()
            .Configure<Animal>(t => t
                .HasRequiredProperty(_ => _.Age, p => p.HasName("a")))
            .Configure<Cat>(t => t
                .HasRequiredProperty(_ => _.Paws, p => p.HasName("p")))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var cat = JsonConvert.DeserializeObject<Cat>(json, settings);

        Assert.Equal(5, cat.Age);
        Assert.Equal(4, cat.Paws);
    }

    [Fact]
    public void DeserializeConfiguredTypeNamesTest()
    {
        var json = "[{'$type':'c'},{'$type':'b'}]";

        var config = Configurator
            .Create()
            .Configure<Cat>(t => t.HasContractName("c"))
            .Configure<Bird>(t => t.HasContractName("b"))
            .Complete();

        var settings = new JsonSerializerSettings
        {
            SerializationBinder = config,
            TypeNameHandling = TypeNameHandling.All,
        };

        var animals = JsonConvert.DeserializeObject<Animal[]>(json, settings);

        Assert.Collection(
            animals,
            a => Assert.IsType<Cat>(a),
            a => Assert.IsType<Bird>(a));
    }

    [Fact]
    public void DeserializeWithCustomDefaultCreatorTest()
    {
        var customConstructorCalled = false;

        var config = Configurator
            .Create()
            .Configure<Cat>(t => t
                .ConstructWith(() =>
                {
                    customConstructorCalled = true;
                    return new Cat { Age = 123 };
                }))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var cat = JsonConvert.DeserializeObject<Cat>("{}", settings);

        Assert.True(customConstructorCalled);
        Assert.Equal(123, cat.Age);
    }

    [Fact]
    public void DeserializeWithCustomOverrideCreatorTest()
    {
        var customConstructorCalled = false;

        var json = "{'a':1,'b':2,'tail':34,'age':12,'f':6}";

        var config = Configurator
            .Create()
            .Configure<Dog>(t => t
                .ConstructWith(pp =>
                {
                    customConstructorCalled = true;
                    return new Dog((int)pp[0], (int)pp[1]);
                }))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var dog = JsonConvert.DeserializeObject<Dog>(json, settings);

        Assert.True(customConstructorCalled);
        Assert.Equal(12, dog.Age);
        Assert.Equal(34, dog.Tail);
    }

    [Fact]
    public void DeserializeWithConstructorExpressionOverrideCreatorTest()
    {
        var json = "{'age':123,'b':2,'f':6}";

        var x = 444;

        var config = Configurator
            .Create()
            .Configure<Animal>(t => t
                .IgnoreProperty(_ => _.Age))
            .Configure<Dog>(t => t
                .ConstructWith<Func<int, int, int, Dog>>((b, age, f) => new Dog(b, x))
                .IgnoreProperty(_ => _.Tail))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var dog = JsonConvert.DeserializeObject<Dog>(json, settings);

        Assert.Equal(2, dog.Age);
        Assert.Equal(444, dog.Tail);
    }

    [Fact]
    public void DeserializeWithMethodCallExpressionOverrideCreatorTest()
    {
        var json = "{'a':1,'b':2,'c':3}";

        var config = Configurator
            .Create()
            .Configure<Animal>(t => t
                .IgnoreProperty(_ => _.Age))
            .Configure<Dog>(t => t
                .ConstructWith<Func<int, int, int, Dog>>((a, b, c) => CreateDog(b, c))
                .IgnoreProperty(_ => _.Tail))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var dog = JsonConvert.DeserializeObject<Dog>(json, settings);

        Assert.Equal(2, dog.Age);
        Assert.Equal(3, dog.Tail);
    }

    static Dog CreateDog(int age, int tail)
        => new Dog(age, tail);

    [Fact]
    public void SerializeWithConditionTest()
    {
        var cats = new Cat[]
        {
            new() { Age = 4 },
            new() { Age = 5 },
            new() { Age = 6 },
        };

        var config = Configurator
            .Create()
            .Configure<Animal>(t => t
                .HasOptionalProperty(_ => _.Age, p => p
                    .HasSerializeCondition(c => c.Age % 5 is 0)))
            .Configure<Cat>(t => t
                .IgnoreProperty(_ => _.Paws))
            .Complete();

        var settings = new JsonSerializerSettings { ContractResolver = config };

        var json = JArray.FromObject(cats, JsonSerializer.Create(settings));

        Assert.Empty(json[0]);
        Assert.Equal(5, json[1].Value<int>("Age"));
        Assert.Empty(json[2]);
    }

    private abstract class Animal
    {
        public int Age { get; set; }
    }

    private sealed class Cat : Animal
    {
        public int Paws { get; set; }
    }

    private sealed class Bird : Animal
    {
        public int Wings { get; set; }
    }

    private sealed class Dog : Animal
    {
        public Dog(int age, int tail)
        {
            Age = age;
            Tail = tail;
        }

        public int Tail { get; set; }
    }
}
