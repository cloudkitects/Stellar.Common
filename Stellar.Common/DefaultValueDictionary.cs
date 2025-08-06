using System.Collections;

namespace Stellar.Common;

/// <summary>
/// A dictionary that returns the default value when accessing keys that do not exist in the dictionary.
/// </summary>
public class DefaultValueDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly IDictionary<TKey, TValue> dictionary;

    #region constructors
    /// <summary>Initializes with an existing dictionary and an equality comparer.</summary>
    /// <param name="dictionary"></param>
    public DefaultValueDictionary(IDictionary<TKey, TValue>? dictionary = null, IEqualityComparer<TKey>? comparer = null)
    {
        dictionary ??= new Dictionary<TKey, TValue>();
        
        this.dictionary = comparer is null
            ? new Dictionary<TKey, TValue>(dictionary)
            : new Dictionary<TKey, TValue>(dictionary, comparer);
    }
    #endregion

    #region IDictionary<TKey, TValue>
    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys => dictionary.Keys;

    public bool Remove(TKey key)
    {
        return dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return dictionary.TryGetValue(key, out value!);
    }

    public ICollection<TValue> Values => dictionary.Values;

    public TValue this[TKey key] {
        get
        {
            dictionary.TryGetValue(key, out var value);

            return value!;
        }
        set => dictionary[key] = value;
    }
    #endregion

    #region ICollection<KeyValuePair<TKey, TValue>>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        dictionary.Add(item);
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        dictionary.CopyTo(array, arrayIndex);
    }

    public int Count => dictionary.Count;

    public bool IsReadOnly => dictionary.IsReadOnly;

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return dictionary.Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey, TValue>>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }
    #endregion

    #region IEnumerable Members
    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }
    #endregion
}
