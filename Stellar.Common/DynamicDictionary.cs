using System.Collections;

namespace Stellar.Common;

/// <summary>
/// A dictionary implementation based on <see cref="DynamicInstance"/>.
/// </summary>
public class DynamicDictionary(IDictionary<string, object?>? dictionary = null, IEqualityComparer<string>? comparer = null) : DynamicInstance(dictionary, comparer), IDictionary<string, object?>
{
    #region IDictionary
    public void Add(string key, object? value)
    {
        dictionary.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return dictionary.ContainsKey(key);
    }

    public ICollection<string> Keys => [.. dictionary.Keys];

    public bool Remove(string key)
    {
        return dictionary.Remove(key);
    }

    public bool TryGetValue(string key, out object? value)
    {
        return dictionary.TryGetValue(key, out value!);
    }

    public ICollection<object?> Values => [.. dictionary.Values];

    public object? this[string key]
    {
        get
        {
            dictionary.TryGetValue(key, out var value);

            return value!;
        }
        set => dictionary[key] = value;
    }
    #endregion

    #region ICollection
    public void Add(KeyValuePair<string, object?> item)
    {
        dictionary.Add(item);
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, object?> item)
    {
        return dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        dictionary.CopyTo(array, arrayIndex);
    }

    public int Count => dictionary.Count;

    public bool IsReadOnly => dictionary.IsReadOnly;

    public bool Remove(KeyValuePair<string, object?> item)
    {
        return dictionary.Remove(item);
    }

    public Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>(dictionary);
    }
    #endregion

    #region IEnumerable
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }
    #endregion
}
