using System.Collections;

namespace Stellar.Common;

/// <summary>
/// A dictionary implementation based on <see cref="DynamicInstance"/>.
/// </summary>
public class DynamicDictionary(IDictionary<string, object>? dictionary = null, IEqualityComparer<string>? comparer = null) : DynamicInstance(dictionary, comparer), IDictionary<string, object>
{
    #region IDictionary<string, object>
    public void Add(string key, object value)
    {
        Dictionary.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return Dictionary.ContainsKey(key);
    }

    public ICollection<string> Keys => Dictionary.Keys;

    public bool Remove(string key)
    {
        return Dictionary.Remove(key);
    }

    public bool TryGetValue(string key, out object value)
    {
        return Dictionary.TryGetValue(key, out value!);
    }

    public ICollection<object> Values => Dictionary.Values;

    public object this[string key]
    {
        get
        {
            Dictionary.TryGetValue(key, out var value);

            return value!;
        }
        set => Dictionary[key] = value;
    }
    #endregion

    #region ICollection<KeyValuePair<string, object>>
    public void Add(KeyValuePair<string, object> item)
    {
        Dictionary.Add(item);
    }

    public void Clear()
    {
        Dictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return Dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        Dictionary.CopyTo(array, arrayIndex);
    }

    public int Count => Dictionary.Count;

    public bool IsReadOnly => Dictionary.IsReadOnly;

    public bool Remove(KeyValuePair<string, object> item)
    {
        return Dictionary.Remove(item);
    }
    #endregion

    #region IEnumerable<KeyValuePair<string,object>>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }
    #endregion

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }
    #endregion
}
