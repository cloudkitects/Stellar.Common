using System.Collections.Generic;

namespace Stellar.Common.Tests;

public class DynamicDictionaryTests
{
    [Fact]
    public void NonexistentPropertyIsNull()
    {
        dynamic obj = new DynamicDictionary();

        var firstName = obj.FirstName;

        Assert.Null(firstName);
    }

    [Fact]
    public void CreatesProperty()
    {
        dynamic obj = new DynamicDictionary();

        obj.FirstName = "Clark";

        Assert.Equal("Clark", obj.FirstName);
    }

    [Fact]
    public void Downcasts()
    {
        dynamic obj = new DynamicDictionary();

        obj.FirstName = "Clark";
        obj["Parts Hair to the"] = "right";

        var dictionary = (IDictionary<string, object>)obj;

        Assert.NotNull(dictionary);

        dictionary["LastName"] = "Kent";
        dictionary.Add("Age", int.MaxValue);

        Assert.Equal("Clark", obj["FirstName"]);
        Assert.Equal(dictionary["FirstName"], obj.FirstName);
        Assert.Equal(dictionary["LastName"], obj.LastName);
        Assert.Equal(dictionary["Age"], obj.Age);
        Assert.Equal("right", dictionary["Parts Hair to the"]);
    }

    [Fact]
    public void PropertiesAreCaseInsensitive()
    {
        var input = new Dictionary<string, object>() { { "FirstName", "Clark" } };

        dynamic obj = new DynamicDictionary(input);

        obj.LastName = "Kent";

        Assert.Equal("Clark", obj.FIRSTNAME);
        Assert.Equal("Clark", obj.firstname);
        Assert.Equal("Clark", obj.fIrStNaMe);
        Assert.Equal("Kent", obj.lastname);
    }

    [Fact]
    public void Composes()
    {
        dynamic obj = new DynamicDictionary();

        obj.Customer = new { FirstName = "Clark", LastName = "Kent" };

        Assert.Equal("Clark", obj.Customer.FirstName);
    }
}