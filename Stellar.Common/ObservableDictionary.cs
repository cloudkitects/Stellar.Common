using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;

using Stellar.Common.Resources;

namespace Stellar.Common;

/// <summary>
/// A generic observable (and indexable) dictionary.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
[Serializable]
public class ObservableDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>, IReadOnlyList<TValue>, INotifyCollectionChanged where TKey : notnull
{
    #region fields
    // a concurrent dictionary keeps keys and indices in sync--not for thread safety purposes
    private readonly ConcurrentDictionary<TKey, int> dictionary;

    // a simple value list maximizes access by index performance and minimizes memory footprint
    private readonly List<TValue> list;
    #endregion

    #region public constructors
    public ObservableDictionary()
        : this(null, 0, EqualityComparer<TKey>.Default)
    {
    }

    public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        : this(null, collection, EqualityComparer<TKey>.Default)
    {
    }

    public ObservableDictionary(IEqualityComparer<TKey> comparer)
        : this(null, 0, comparer)
    {
    }

    public ObservableDictionary(int concurrencyLevel, int capacity)
        : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default)
    {
    }

    public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        : this(null, collection, comparer)
    {
    }

    public ObservableDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        : this((int?)concurrencyLevel, capacity, comparer)
    {
    }

    public ObservableDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        : this((int?)concurrencyLevel, collection, comparer)
    {
    }
    #endregion

    #region private constructors
    private ObservableDictionary(int? concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        dictionary = concurrencyLevel == null
            ? new ConcurrentDictionary<TKey, int>(comparer)
            : new ConcurrentDictionary<TKey, int>(concurrencyLevel.Value, capacity, comparer);

        list = new List<TValue>(capacity);
    }

    private ObservableDictionary(int? concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);

        ArgumentNullException.ThrowIfNull(comparer);

        var count = 
            (collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ??
            (collection as IReadOnlyCollection<KeyValuePair<TKey, TValue>>)?.Count;

        dictionary = concurrencyLevel == null
            ? new ConcurrentDictionary<TKey, int>(comparer)
            : new ConcurrentDictionary<TKey, int>(concurrencyLevel.Value, count ?? 31, comparer);

        list = new List<TValue>(count ?? 0);

        foreach (var (key, value) in collection)
        {
            if (!dictionary.TryAdd(key, list.Count))
            {
                throw new ArgumentException(Exceptions.SourceCannotContainDuplicateKey, nameof(collection));
            }

            list.Add(value);
        }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Map a key to it's index.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The index that corresponds to the given key or -1 if there's no such key.</returns>
    public int IndexOf(TKey key)
    {
        if (!dictionary.TryGetValue(key, out var index))
        {
            return -1;
        }

        return index;
    }
    #endregion

    #region IDictionary<TKey, TValue> members
    public TValue this[TKey key]
    {
        get => list[dictionary[key]];
        set
        {
            var action = NotifyCollectionChangedAction.Add;

            dictionary.AddOrUpdate(key,
                k =>
                {
                    list.Add(value);

                    return list.Count - 1;
                },
                (k, index) =>
                {
                    action = NotifyCollectionChangedAction.Replace;

                    list[index] = value;

                    return index;
                });

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, new KeyValuePair<TKey, TValue>(key, value)));
        }
    }

    public int Count => list.Count;
    public bool IsReadOnly => false;
    public ICollection<TKey> Keys => dictionary.Keys;
    public ICollection<TValue> Values => list;

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Add(TKey key, TValue value)
    {
        if (!dictionary.TryAdd(key, list.Count))
        {
            return;
        }

        list.Add(value);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
    }

    public void Clear()
    {
        list.Clear();
        dictionary.Clear();

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        var (key, value) = item;

        return dictionary.TryGetValue(key, out var index) && Equals(list[index], value);
    }

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (index < 0 || array.Length - Count < index)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        foreach (var keyValue in this)
        {
            array[index] = keyValue;

            index++;
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!dictionary.TryRemove(item.Key, out var index))
        {
            return false;
        }

        if (Equals(list[index], item.Value))
        {
            list.RemoveAt(index);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return true;
        }

        dictionary[item.Key] = index;

        return false;
    }

    public bool Remove(TKey key)
    {
        if (!dictionary.TryRemove(key, out var index))
        {
            return false;
        }

        var value = list[index];

        list.RemoveAt(index);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));

        return true;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!dictionary.TryGetValue(key, out var index))
        {
            value = default!;

            return false;
        }

        value = list[index];

        return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.Select(keyIndex => new KeyValuePair<TKey, TValue>(keyIndex.Key, list[keyIndex.Value])).GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region IReadOnlyList<TValue> members
    public TValue this[int index] => list[index];

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => list.GetEnumerator();
    #endregion

    #region INotifyCollectionChanged members
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    #endregion
}