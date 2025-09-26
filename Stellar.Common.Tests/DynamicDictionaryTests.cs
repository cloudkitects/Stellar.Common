using Microsoft.CSharp.RuntimeBinder;
using System.Linq;

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
        var input = new Dictionary<string, object?>() { { "FirstName", "Clark" } };

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

    [Fact]
    public void PropertiesTest()
    {
        var input = new Dictionary<string, object?>() {
            { "First", "Clark" },
            { "Last", "Kent" },
            { "Parts", "Left" }
        };

        var alterego = new KeyValuePair<string, object?>("Alterego", "Superman");

        dynamic obj = new DynamicDictionary(input);

        obj.Age = int.MaxValue;
        obj[alterego.Key] = alterego.Value;

        Assert.Equal("Clark", obj.First);
        Assert.Equal("Kent", obj.Last);
        Assert.Equal("Left", obj.Parts);
        Assert.Equal(int.MaxValue, obj.Age);
        Assert.Equal("Superman", obj.Alterego);
        Assert.Null(obj.Bogus);
    }

    [Fact]
    public void EnumeratorTest()
    {
        var input = new Dictionary<string, object?>() {
            { "First", "Clark" },
            { "Last", "Kent" },
            { "Parts", "Left" },
            { "Alter", "Superman" }
        };

        dynamic obj = new DynamicDictionary(input);

        var enumerator1 = ((IEnumerable<KeyValuePair<string, object?>>)obj).GetEnumerator();
        var enumerator2 = obj.GetEnumerator();

        Assert.True(enumerator1.MoveNext());
        Assert.Equal("First", $"{enumerator1.Current.Key}");
        Assert.Equal("Clark", $"{enumerator1.Current.Value}");
        
        Assert.True(enumerator2.MoveNext());
        Assert.True(enumerator2.MoveNext());
        Assert.True(enumerator2.MoveNext());
        Assert.Equal("Parts", $"{enumerator2.Current.Key}");
        Assert.Equal("Left", $"{enumerator2.Current.Value}");
    }

    [Fact]
    public void DefaultValueDictionaryTest()
    {
        var input = new Dictionary<string, object?>() {
            { "First", "Clark" },
            { "Last", "Kent" },
            { "Parts", "Left" },
            { "Alter", "Superman" }
        };

        var dict = new DefaultValueDictionary<string, object?>(input);

        foreach (var kvp in dict)
        {
            Assert.Equal(input[kvp.Key], kvp.Value);
        }
    }

    [Theory]
    [InlineData("Keys", "one,two")]
    [InlineData("IsReadonly", true)]
    [InlineData("Count", 30)]
    public void PropertyCollisionTest(string key, object? value)
    {
        dynamic obj = new DynamicDictionary(new Dictionary<string, object?>() { { key, value } });

        // intrinsic properties are not overriden
        Assert.NotEqual(value, obj.Keys);
        Assert.False(obj.IsReadOnly);
        Assert.Equal(1, obj.Keys.Count);
        Assert.Equal(1, obj.Count);

        // colliding user properties are available
        Assert.Equal(value, obj[key]);
        Assert.Equal(value, obj.Values[0]); 
    }

    [Theory]
    [InlineData("Add", "oh my")]
    [InlineData("Contains", false)]
    [InlineData("ContainsKey", "certainly")]
    [InlineData("TryGetValue", -1 /* beer */)]
    [InlineData("Remove", "2025-08-16")]
    [InlineData("Clear", "Crystal")]
    public void MethodCollisionTest(string key, object? value)
    {
        var input = new Dictionary<string, object?>() { { key, value } };
        
        dynamic obj = new DynamicDictionary(input);

        // intrinsic methods are overriden when used without parameters...
        Assert.Equal(value, obj.Add ?? obj.Clear ?? obj.Contains ?? obj.ContainsKey ?? obj.TryGetValue ?? obj.Remove);

        // ...but still work when used with parameters
        var kvp = new KeyValuePair<string, object?>("Well", "done");
        
        obj.Add(kvp.Key, kvp.Value);
        Assert.True(obj.Contains(input.First()));
        Assert.True(obj.Contains(kvp));
        Assert.True(obj.ContainsKey(key));
        Assert.True(obj.TryGetValue(key, out object? v) && v == value);
        Assert.True(obj.Remove(input.First()));
        Assert.Equal(1, obj.Count);

        obj.Remove(key);
        Assert.Equal(1, obj.Count);

        obj.Clear();
        Assert.Equal(0, obj.Count);

        obj.Focus = new Func<string>(() => "pocus");

        Assert.Equal("pocus", obj.Focus());
        try
        {
            obj.Locus();
        }
        catch (RuntimeBinderException)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void CovertsToDictionary()
    {
        var input = new Dictionary<string, object?>() {
            { "First", "Clark" },
            { "Last", "Kent" },
            { "Parts", "Left" },
            { "Alter", "Superman" }
        };

        dynamic obj = new DynamicDictionary(input);

        Dictionary<string, object?> output = obj.ToDictionary();

        Assert.Equal(4, output.Keys.Count);
        Assert.Equal(4, output.Values.Count);

        foreach(KeyValuePair<string, object?> kvp in obj)
        {
            Assert.Equal(output[kvp.Key], kvp.Value);
        }
    }

    [Fact]
    public void CopiesToArray()
    {
        var input = new Dictionary<string, object?>() {
            { "Name", "Spiderman" },
            { "Born", new DateOnly(1972, 3, 15) },
            { "Cape", 0.0 },
            { "Flies", false }
        };

        dynamic obj = new DynamicDictionary(input);

        obj.Motto = "with great power...";

        var array = new KeyValuePair<string, object?>[5];

        obj.CopyTo(array, 0);

        Assert.Equal(5, array.Length);
        Assert.Equal(obj.Motto, array[4].Value);
    }
}