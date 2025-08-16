using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Stellar.Common;

/// <summary>A <see cref="Type" /> metadata cache.</summary>
public static class TypeCache
{
    /// <summary>
    /// Cache that stores types as the key and the type's PropertyInfo and FieldInfo in a <see cref="OrderedDictionary"/> as the value.
    /// </summary>
    public static readonly Dictionary<Type, OrderedDictionary> Cache = [];

    /// <summary>Gets and caches a type's properties and fields.</summary>
    public static OrderedDictionary GetOrAdd(Type type, Func<MemberInfo, bool>? ignore = null)
    {
        if (Cache.TryGetValue(type, out OrderedDictionary? value))
        {
            return value;
        }

        var typeMetadata = new OrderedDictionary(StringComparer.InvariantCultureIgnoreCase);

        var properties = type.GetProperties();

        foreach (var propertyInfo in properties)
        {
            if (!(ignore?.Invoke(propertyInfo) ?? false))
            {
                typeMetadata[propertyInfo.Name] = propertyInfo;
            }
        }

        var fields = type.GetFields();

        foreach (var fieldInfo in fields)
        {
            if (!(ignore?.Invoke(fieldInfo) ?? false))
            {
                typeMetadata[fieldInfo.Name] = fieldInfo;
            }
        }

        Cache.Add(type, typeMetadata);

        return typeMetadata;
    }

    /// <summary>Try getting a cached type.</summary>
    public static bool TryGet(Type type, out OrderedDictionary? result)
    {
        return Cache.TryGetValue(type, out result);
    }


    /// <summary>Get a dictionary containing the objects property and field names and values.</summary>
    public static IDictionary<string, object?> ToDictionary(object instance)
    {
        // support dynamic objects backed by a dictionary of string object
        if (instance is IDictionary<string, object?> asDictionary)
        {
            return asDictionary;
        }

        var dictionary = new Dictionary<string, object?>();

        foreach (DictionaryEntry entry in GetOrAdd(instance.GetType()))
        {
            var value = entry.Value switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(instance),
                PropertyInfo propertyInfo => propertyInfo.GetValue(instance, null),
                _ => throw new InvalidOperationException($"Unexpected reflection class found in {nameof(instance)}.") // unreachable
            };

            dictionary.Add($"{entry.Key}", value);
        }

        return dictionary;
    }
}
