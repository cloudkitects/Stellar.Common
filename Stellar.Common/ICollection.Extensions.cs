namespace Stellar.Common;

public static partial class Extensions
{
    #region array
    public static T[] InsertAt<T>(this T[] source, int index, T value, bool trim = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<T>(source);

        list.Insert(index, value);

        while (trim && list.Count > 0 && list[^1] is null)
        {
            list.RemoveAt(list.Count - 1);
        }

        return [.. list];
    }

    public static T[] Remove<T>(this T[] source, T value, bool trim = false)
    {
        TryRemove(source, value, trim, out var result);

        return result;
    }

    public static bool TryRemove<T>(this T[] source, T value, bool trim, out T[] result)
    {
        ArgumentNullException.ThrowIfNull(source);

        var index = Array.IndexOf(source, value);

        if (index < 0)
        {
            result = source;

            return false;
        }

        result = RemoveAt(source, index, trim);

        return true;
    }

    public static T[] RemoveAt<T>(this T[] source, int index, bool trim = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<T>(source);

        list.RemoveAt(index);

        while (trim && list.Count > 0 && list[^1] is null)
        {
            list.RemoveAt(list.Count - 1);
        }

        return [.. list];
    }

    public static bool IsNullOrEmpty<T>(this T[] source)
    {
        return source is null || source.Length == 0;
    }

    public static bool TryGetValueAt<T>(this T[] source, int index, out T value)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (index < 0 || index >= source.Length)
        {
            value = default!;

            return false;
        }

        value = source[index];

        return true;
    }
    #endregion

    #region dictionary
    public static Dictionary<string, object?> Slice(this IDictionary<string, object?> dictionary, IEnumerable<string> keys)
    {
        return dictionary.Where(entry => keys.Contains(entry.Key))
            .ToDictionary(entry => entry.Key, kv => kv.Value);
    }

    public static Dictionary<string, object?> Splice(this IDictionary<string, object?> dictionary, IEnumerable<string> keys)
    {
        return dictionary.Where(entry => !keys.Contains(entry.Key))
            .ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public static Dictionary<string, object?> Merge(this IDictionary<string, object?> target, IDictionary<string, object?> source)
    {
        var result = new Dictionary<string, object?>(target);

        foreach (var kvp in source)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    public static void AddOrUpdate(this IDictionary<string, object?> dictionary, string key, object? value)
    {
        try
        {
            dictionary.Add(key, value);
        }
        catch
        {
            dictionary[key] = value;
        }
    }
    #endregion

    #region collection
    public static Guid Hash<T>(this ICollection<T> input)
    {
        return string.Join(':', input).Hash();
    }
    #endregion
}
