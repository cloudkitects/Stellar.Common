# Stellar.Common
Extensions and utility library supporting the Stellar family of products, mostly subtracted from [Stellar.IO](https://github.com/cloudkitects/Stellar.IO) and [Stellar.DAL](https://github.com/cloudkitects/Stellar.DAL) Copyright (c) 2025 Cloudkitects, LLC under MIT License.

These classes and extensions tend to common Extract Transform and Load (ETL) use cases, lower the barrier between applications and data repositories and support ETL-as-Code.

# Bucket

A bucket wraps a `<TKey, bool>` dictionary initialized with a set of distinct expected elements. As elements are added to it, the bucket flips the bits and is eventually full:

```cs
var bucket = new Bucket<string>([ "apples", "oranges", "bananas" ]);

bucket.Add("apples");

Assert.True(bucket["apples"]);
Assert.False(bucket["oranges"]);
Assert.False(bucket["bananas"]);
        
Assert.False(bucket.IsFull);

bucket.Add("oranges");
bucket.Add("bananas");

if (bucket.IsFull)
{
    // peel and chop!
}
```

A real-world use case kicks off the next ETL hop once expected files are loaded in batch processes, e.g., refresh the model when today's `customer`, `accounts` and `balances` files are loaded.

In essence, it is a user-defined synchronous stand-in for `Task.WhenAll()`. It is _not_ a multi-process orchestrator, as state cannot be preserved across processes.

# Buffer

A generic random-access buffer initialized to the given. Extirpated from Stellar.IO, it essentially supports I/O readers with a buffer fill callback and a bookmark equality comparer.

```cs
// buffer fill callback wraps Read() handling range and EOF...
private int BufferFillCallback(char[] buffer, int offset) 
{
    ArgumentNullException.ThrowIfNull(buffer);

    if (offset < 0 || buffer.Length <= offset)
    {
        throw new ArgumentOutOfRangeException(nameof(offset));
    }

    if (EOF)
    {
        return 0;
    }

    var count = TextReader.Read(buffer, offset, buffer.Length - offset);

    EOF = count <= 0;

    return count;
}

// somewhere within the reader ctor...
buffer = new Buffer<char>(bufferSize, BufferFillCallback);

// buffer supports token-based reading
protected override ReadResult ReadCore(Buffer<char> buffer, IList<string> values)
{
    while (buffer.Refill() && !IsNewLine(buffer.Current))
    {
        ...
    }
```

# DefaultValueDictionary

Implements `IDictionary<TKey, TValue> where TKey : notnull` and returns the `TValue?` default when keys do not exist. In other words, a loosely-defined dictionary.

The constructor optionally takes in a dictionary and a `TKey` equality comparer that comes in handy to ignore case-sensitivity.

Extirpated from the DAL, it essentially supports `DynamicInstance`.

# DynamicInstance

Implements a `DynamicObject` overiding call-site binders to return entries from a private default value dictionary.

```cs
// a built-in dynamic object throws runtime binder exceptions
// for mispelled or non-initialized properties...

dynamic person = new { Age = 42 };

var age = person.age;   // ...throws (person does not contain a definition for 'age')...
var name = person.Name; // ...throws (person does not contain a definition for 'Name')...

// ...whereas a dynamic instance that ignores case is very forgiving :)

var person = new DynamicInstance();

var first = person.FirstName; // first == null

person.Alterego = "Superman";
person["PARTS HAIR TO THE"] = "Right";

Assert.Equals("Superman", person.ALTEREGO);
Assert.Equals("Right", person["parts hair to the"]);
```

Strongly-typed yet loosely-defined objects can help reduce impedance between objects and data transfer object (DTOs). We call this the "BLT no tomato" impedance problem: either the API delivers a BLT and the UI ignores the tomato, or the API delivers a `null` for tomato but the UI doesn't break. In other words, the API and the UI are allowed to disagree on the definition of objects without throwing a fit.

# DynamicDictionary

The DAL leverages dynamic dictionaries as controller sit-ins in the Model-View-Controller (MVC) pattern and for Rapid Application Development (RAD) use cases:
```cs
// somewhere in a model...

public static List<dynamic> GetReport(string period)
{
    return GetCommand()
        .SetCommandText(report_sql)
        .AddParameter("period", period)
        .ExecuteToDynamicList();
}

// somewhere in a view...

var entries = model.GetReport("2025");

foreach (var entry in entries)
{
    Report.AddNewRow();

    Report.AddCell((string)entry.period, 0, align: TextAlignment.End);
    Report.AddCell((string)entry.product, 1, align: TextAlignment.Start);
    Report.AddCell($"{entry.inventory:N0}", 2);
    Report.AddCell($"{entry.sold:N0}", 3);
    Report.AddCell($"{entry.sales:C}", 4, align: TextAlignment.End);
    Report.AddCell($"{entry.fees:C}", 5, align: TextAlignment.End);
    Report.AddCell($"{entry.profit:C}", 6, align: TextAlignment.End);
}

// ...controller'd be like... wth?
```

The controller is optional, or can be leveraged for converting the DTO into a strongly-defined object if so desired. Using loosely-defined objects is a quick way to get things building. This is of course a double-edged sword, as it can still lead to runtime errors and information loss.

User property names colliding with method names are handled gracefully (in a particle/wave duality manner):

```cs
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

    // colliding property names are available...
    Assert.Equal(value, obj.Add ?? obj.Clear ?? obj.Contains ?? obj.ContainsKey ?? obj.TryGetValue ?? obj.Remove);

    // ...methods are also available.
    var kvp = new KeyValuePair<string, object?>("Well", "done");
        
    obj.Add(kvp.Key, kvp.Value);
    Assert.True(obj.Contains(input.First()));
    Assert.True(obj.Contains(kvp));
    Assert.True(obj.ContainsKey(key));
    Assert.True(obj.TryGetValue(key, out object? v) && v == value);
    Assert.True(obj.Remove(input.First()));
    Assert.Equal(1, obj.Count);
}
```

User v. intrinsic property name collisions are handled less gracefully, but still work as expected:
```cs
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
```cs

# EnumHelper

An in-depth mapper of declared string or underlying values to enum members, with helpers checking if a value is defined, getting member names, etc. It aims at reducing the impedance between ETL-as-Code (JSON, YAML) and CSharp.

# Extensions

Safe versions of intrinsic methods (try/catch wrappers with fallback values), a cryptographic hash helper and a few other helpers used throughout the Stellar family.

Collection extensions include:

- Wrapping an array in a list to enable "insert at", "remove at" operations, and "null or empty" sugar.
- Dictionary slice (reduce to the specified keys), splice (remove the specified keys) and merge (augment or update a dictionary with another).

# FileInfoEx

Wraps `FileInfo` with a few safe wrappers and file name metadata. Used by the `FileNameParser`, next.

# FileNameParser

Another ETL chore, this class parses a file name using a regular expression with both built-in and custom groups to produce a new file name based on a template. Built-in placeholders include file metadata including the file name, base file name (file name without extension), created and modified date times and file name segments identiying parts such as type and timestamp. 

```cs
var result = FileNameParser.TryParse(
    filename: "Hello20241231.txt",
    pattern: "Hello(?<timestamp>.+).txt",
    template: "{timestamp:yyyy-MM-dd}-hello.tsv", out var output);

Assert.Equal("2024-12-31-hello.tsv", output.Filename);
```
