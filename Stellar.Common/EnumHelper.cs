using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

using Stellar.Common.Resources;

namespace Stellar.Common;

public static class EnumHelper
{
    private class EnumInfo
    {
        public bool IsFlags;
        public ulong AllFlags;

        public required string[] Names = [];
        public required ulong[] Values = [];
    }

    private static readonly ConcurrentDictionary<Type, EnumInfo> Cache = new();

    private static void AssertIsEnum(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsEnum)
        {
            return;
        }

        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.NotAnEnumType, type), nameof(type));
    }

    /// <summary>
    /// Gets the <see cref="EnumInfo"/> structure for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Enum"/> type to inspect.</param>
    /// <returns>The <see cref="EnumInfo"/> structure for the provided <paramref name="type"/>.</returns>
    private static EnumInfo GetEnumInfo(Type type)
    {
        AssertIsEnum(type);

        return Cache.GetOrAdd(type,
            type =>
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                var names = new string[fields.Length];
                var values = new ulong[fields.Length];

                for (var i = 0; i < fields.Length; i++)
                {
                    names[i] = fields[i].Name;
                    values[i] = Convert.ToUInt64(fields[i].GetValue(null), NumberFormatInfo.InvariantInfo);
                }

                for (var i = 1; i < values.Length; i++)
                {
                    var value = values[i];
                    var name = names[i];

                    var j = i;

                    while (j > 0 && values[j - 1] > value)
                    {
                        values[j] = values[j - 1];
                        names[j] = names[j - 1];

                        j--;
                    }

                    values[j] = value;
                    names[j] = name;
                }

                var info = new EnumInfo { Names = names, Values = values };

                if (!Attribute.IsDefined(type, typeof(FlagsAttribute)))
                {
                    return info;
                }

                info.IsFlags = true;

                foreach (var v in info.Values)
                {
                    info.AllFlags |= v;
                }

                return info;
            });
    }

    public static string? GetName<T>(T value) where T : struct
    {
        return GetName(typeof(T), value);
    }

    public static string? GetName(Type type, object value)
    {
        AssertIsEnum(type);

        var info = GetEnumInfo(type);

        var index = Array.BinarySearch(info.Values!, (ulong)value);

        return index > -1
            ? (info.Names[index])
            : null;
    }

    public static string[] GetNames<T>() where T : struct
    {
        return GetNames(typeof(T));
    }

    public static string[] GetNames(Type type)
    {
        AssertIsEnum(type);

        var info = GetEnumInfo(type);

        var names = new string[info.Names.Length];
        
        Array.Copy(info.Names, names, names.Length);

        return names;
    }

    public static T[] GetValues<T>() where T : struct
    {
        var type = typeof(T);

        AssertIsEnum(type);

        var info = GetEnumInfo(type);

        var values = new T[info.Values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            values[i] = (T)Enum.ToObject(type, info.Values[i]);
        }

        return values;
    }

    public static Array GetValues(Type type)
    {
        AssertIsEnum(type);

        var info = GetEnumInfo(type);

        var values = Array.CreateInstance(type, info.Values.Length);

        for (var i = 0; i < values.Length; i++)
        {
            values.SetValue(Enum.ToObject(type, info.Values[i]), i);
        }

        return values;
    }

    public static bool IsDefined<T>(object value) where T : struct
    {
        return IsDefined(typeof(T), value);
    }

    public static bool IsDefined(Type type, object value)
    {
        AssertIsEnum(type);

        var info = GetEnumInfo(type);

        var val = Convert.ToUInt64(value, CultureInfo.InvariantCulture);

        var index = Array.BinarySearch(info.Values, val);

        if (index > -1)
        {
            return true;
        }

        if (info.IsFlags)
        {
            return (val & info.AllFlags) == val;
        }

        return false;
    }

    public static T Parse<T>(string input, bool ignoreCase) where T : struct
    {
        if (TryParse(input, ignoreCase, out T value))
        {
            return value;
        }

        throw new ArgumentException(string.Empty, nameof(input));
    }

    public static bool TryParse<T>(string input, bool ignoreCase, out T? value)
    {
        if (TryParse(typeof(T), input, ignoreCase, out var val))
        {
            value = (T)val!;

            return true;
        }

        value = default;

        return false;
    }

    public static object Parse(Type type, string input, bool ignoreCase)
    {
        if (TryParse(type, input, ignoreCase, out var value))
        {
            return value!;
        }

        throw new ArgumentException(string.Empty, nameof(input));
    }

    public static bool TryParse(Type type, string input, bool ignoreCase, out object? value)
    {
        AssertIsEnum(type);

        if (input == null || (input = input.Trim()).Length == 0)
        {
            value = null;
            return false;
        }

        EnumInfo info;

        if (input[0].IsNumberOrSign())
        {
            if (ulong.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var val))
            {
                info = GetEnumInfo(type);

                if (Array.BinarySearch(info.Values, val) < 0)
                {
                    if (!info.IsFlags || (val & info.AllFlags) != val)
                    {
                        value = null;
                        return false;
                    }
                }

                value = Enum.ToObject(type, val);
                return true;
            }

            value = null;
            return false;
        }

        info = GetEnumInfo(type);

        var validNames = ignoreCase
            ? [.. info.Names.Select(n => n.ToLowerInvariant())]
            : info.Names;

        int index;

        if (info.IsFlags)
        {
            var names = input.Split(',');
            ulong val = 0;

            foreach (var n in names)
            {
                var name = (ignoreCase
                    ? n.ToLowerInvariant()
                    : n).Trim();

                index = Array.IndexOf(validNames, name);

                if (index > -1)
                {
                    val |= info.Values[index];
                }
                else
                {
                    value = null;

                    return false;
                }
            }

            value = Enum.ToObject(type, val);

            return true;
        }

        if (ignoreCase)
        {
            input = input.ToLowerInvariant();
        }

        index = Array.IndexOf(validNames, input);

        if (index > -1)
        {
            value = Enum.ToObject(type, info.Values[index]);
            return true;
        }

        value = null;
        return false;
    }

    public static bool CheckEnumerationValue(object value, bool isFlags, bool throwOnError, string argumentName, params object[] validValues)
    {
        argumentName = string.IsNullOrEmpty(argumentName)
            ? "value"
            : argumentName;

        if (value == null)
        {
            if (throwOnError)
            {
                throw new ArgumentNullException(argumentName);
            }

            return false;
        }

        ulong mask = 0;
        var v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);

        if (validValues != null)
        {
            foreach (var t in validValues)
            {
                var valid = Convert.ToUInt64(t, NumberFormatInfo.InvariantInfo);

                if (v == valid)
                {
                    return true;
                }

                mask |= valid;
            }
        }

        if (isFlags && ((v & mask) == v))
        {
            return true;
        }

        if (throwOnError)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.InvalidEnumArgument, value), argumentName);
        }

        return false;
    }

    public static bool CheckEnumerationValueByMinMax(object value, bool throwOnError, string argumentName, object minValue, object maxValue)
    {
        ArgumentNullException.ThrowIfNull(minValue);
        ArgumentNullException.ThrowIfNull(maxValue);

        argumentName = string.IsNullOrEmpty(argumentName)
            ? "value"
            : argumentName;

        if (value == null)
        {
            if (throwOnError)
            {
                throw new ArgumentNullException(argumentName);
            }

            return false;
        }

        var v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);

        var min = Convert.ToUInt64(minValue, NumberFormatInfo.InvariantInfo);
        var max = Convert.ToUInt64(maxValue, NumberFormatInfo.InvariantInfo);

        if (v >= min && v <= max)
        {
            return true;
        }

        if (throwOnError)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.InvalidEnumArgument, value), argumentName);
        }

        return false;
    }

    public static bool CheckEnumerationValueByMask(object value, bool throwOnError, string argumentName, object mask)
    {
        ArgumentNullException.ThrowIfNull(mask);

        argumentName = string.IsNullOrEmpty(argumentName)
            ? "value"
            : argumentName;

        if (value == null)
        {
            if (throwOnError)
            {
                throw new ArgumentNullException(argumentName);
            }

            return false;
        }

        var v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);
        var m = Convert.ToUInt64(mask, NumberFormatInfo.InvariantInfo);

        if ((v & m) == v)
        {
            return true;
        }

        if (throwOnError)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.InvalidEnumArgument, value), argumentName);
        }

        return false;
    }
}