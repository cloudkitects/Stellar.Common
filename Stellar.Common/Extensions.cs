using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Stellar.Common;

/// <summary>
/// .Net intrinsic object extensions.
/// </summary>
public static partial class Extensions
{
    #region Queue<string>
    /// <summary>
    /// Safe deque.
    /// </summary>
    /// <param name="queue">The queue to dequeue (pop).</param>
    /// <param name="required">Throw behavior.</param>
    /// <returns>the popped element or empty.</returns>
    public static string Pop(this Queue<string> queue, bool required = false)
    {
        if (!queue.TryDequeue(out var head) || required && string.IsNullOrEmpty(head))
        {
            throw new ArgumentException("A required element is missing or empty.");
        }

        return head;
    }
    #endregion

    #region object
    public static bool IsAnonymousType(this object item)
    {
        return item is not null && item.GetType().Namespace is null;
    }

    /// <summary>
    /// Convert an object into a <see cref="DynamicDictionary"/>, leveraging the type cache.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The object converted to a dynamic dictionary.</returns>
    public static DynamicDictionary ToDynamicDictionary(this object obj)
    {
        return [.. TypeCache.ToDictionary(obj)];
    }
    #endregion

    #region string
    public static string Qualify(this string value, char q = '\"', char e = '\"')
    {
        var r = e == '\\'
            ? $"{e}{e}"
            : $"{e}";

        return $"{q}{Regex.Replace(value, @$"([^{r}]){q}", $"$1{e}{q}")}{q}";
    }

    public static Guid Hash(this string input)
    {
        var hash = MD5.HashData(Encoding.Default.GetBytes(input));

        return new Guid([.. hash.Take(16)]);
    }

    public static string? NullIfEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
    #endregion

    #region numeric
    public static void Sort<T>(ref T min, ref T max) where T : struct, INumber<T>
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }
    }

    public static T Clamp<T>(this T value, T min, T max) where T : struct, INumber<T>
    {
        Sort(ref min, ref max);

        return T.Clamp(value, min, max);
    }

    public static bool Between<T>(this T value, T min, T max) where T : struct, INumber<T>
    {
        Sort(ref min, ref max);

        return
            value.CompareTo(min) >= 0 &&
            value.CompareTo(max) <= 0;
    }
    #endregion

    #region char
    public static bool IsNumber(this char c)
    {
        return char.IsNumber(c);
    }

    public static bool IsNumberOrSign(this char c)
    {
        return char.IsNumber(c) || c == '-' || c == '+';
    }

    public static bool IsIdentifier(this char c)
    {
        return char.IsLetter(c) || c == '_';
    }
    #endregion

    #region date and time
    public static int YearTotalDays(this DateTime datetime)
    {
        var y = datetime.Year;

        var s = new DateTime(y, 1, 1);
        var e = new DateTime(y, 12, 31);

        return Convert.ToInt32((e - s).TotalDays);
    }
    #endregion

    #region culture info
    public static CultureInfo CreateCultureInfo(string name)
    {
        CultureInfo cultureInfo;

        try
        {
            cultureInfo = CultureInfo.CreateSpecificCulture(name);
        }
        catch
        {
            cultureInfo = CultureInfo.CurrentCulture;
        }

        return cultureInfo;
    }
    #endregion

    #region type
    public static bool IsDynamic(this Type type)
    {
        return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)) || type == typeof(object);
    }

    /// <summary>
    /// Cache of value types' default values to reduce Activator.CreateInstance calls to a minimum.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> DefaultValueCache = new();

    /// <summary>Gets the default value for the given type.</summary>
    /// <param name="type">Type to get the default value for.</param>
    /// <returns>Default value of the given type.</returns>
    public static object? GetDefaultValue(this Type type)
    {
        return type.IsValueType
            ? DefaultValueCache.GetOrAdd(type, Activator.CreateInstance!)
            : null;
    }
    #endregion

    #region helpers
    public static bool False(Action action) { action(); return false; }

    public static bool True(Action action) { action(); return true; }
    #endregion
}
