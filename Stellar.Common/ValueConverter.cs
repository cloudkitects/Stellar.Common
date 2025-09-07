using Stellar.Common.Resources;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stellar.Common;

/// <summary>
/// Common value conversion wrapping intrinsic converters
/// with extended functionality.
/// </summary>
public static class ValueConverter
{
    #region styles
    /// <summary>
    /// A composite of Integer == AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign.
    /// </summary>
    public static NumberStyles IntegerNumberStyles { get; set; } = NumberStyles.Integer | NumberStyles.AllowThousands;
    public static NumberStyles FloatingPointNumberStyles { get; set; } = NumberStyles.Float;
    public static DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;
    public static TimeSpanStyles TimeStyles { get; set; } = TimeSpanStyles.None;
    #endregion

    #region datetime formats
    private static string[] datetimeFormats =
    [
        // JavaScript
        "ddd MMM d HH:mm:ss 'GMT'zzz yyyy",
        "ddd MMM d HH:mm:ss 'UTC'zzz yyyy",
        "ddd MMM dd HH:mm:ss 'GMT'zzz yyyy",
        "ddd MMM dd HH:mm:ss 'UTC'zzz yyyy",
        "ddd MMM dd HH:mm:ss 'CDT' yyyy",
        // timestamps
        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",
        "yyyyMMdd"
    ];

    public static string[] GetDateTimeFormats()
    {
        return (string[])datetimeFormats.Clone();
    }

    public static void AddDateTimeFormat(string format)
    {
        AddDateTimeFormats([format]);
    }

    public static void AddDateTimeFormats(IEnumerable<string> formats)
    {
        var list = new List<string>(datetimeFormats);

        list.AddRange(formats);

        Interlocked.Exchange(ref datetimeFormats, [.. list.Distinct()]);
    }
    #endregion

    #region time formats
    private static string[] timeFormats =
    [
        "HHmmss",
        "HHmm"
    ];

    public static string[] GetTimeFormats()
    {
        return (string[])timeFormats.Clone();
    }

    public static void AddTimeFormat(string format)
    {
        AddTimeFormats([format]);
    }

    public static void AddTimeFormats(IEnumerable<string> formats)
    {
        var list = new List<string>(timeFormats);

        list.AddRange(formats);

        Interlocked.Exchange(ref timeFormats, [.. list.Distinct()]);
    }
    #endregion

    #region helpers
    private static string Trim(string value, TrimmingOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if ((options & TrimmingOptions.Both) == TrimmingOptions.Both)
        {
            value = value.Trim();
        }
        else if ((options & TrimmingOptions.End) == TrimmingOptions.End)
        {
            value = value.TrimEnd();
        }
        else if ((options & TrimmingOptions.Start) == TrimmingOptions.Start)
        {
            value = value.TrimStart();
        }

        return value;
    }

    private static T TryOrThrow<T>(bool success, string input, T value)
    {
        return success
            ? value
            : throw new FormatException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionFailed, input, typeof(T)));
    }
    #endregion

    #region to string
    public static string ToString(
        object? value,
        string? format = null,
        string emptyString = "",
        string? codePage = null)
    {
        if (value is null || value == DBNull.Value)
        {
            return emptyString;
        }

        var culture = ParseCultureInfo(codePage);

        var result = value switch
        {
            DateTime datetime => datetime.ToString(format, culture),
            DateTimeOffset offset => offset.ToString(format, culture),
            TimeSpan timespan => timespan.ToString(format, culture),
            DateOnly date => date.ToString(format, culture),
            TimeOnly time => time.ToString(format, culture),

            double d => d.ToString(format, culture),
            float f => f.ToString(format, culture),
            decimal dec => dec.ToString(format, culture),
            int i => i.ToString(format, culture),
            long l => l.ToString(format, culture),
            short s => s.ToString(format, culture),

            byte b => b.ToString(format, culture),
            sbyte sb => sb.ToString(format, culture),
            uint ui => ui.ToString(format, culture),
            ulong ul => ul.ToString(format, culture),
            ushort us => us.ToString(format, culture),

            bool boolean => boolean.ToString(culture),

            char c => c.ToString(culture),

            Guid guid => guid.ToString(format, culture),

            _ => null
        };

        if (result is not null)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return $"{value}";
        }

        var fmt = $"{{0:{format}}}".Replace("0:0", "0");

        return string.Format(culture, fmt, value);
    }
    #endregion

    #region parse
    #region generic
    public static T Parse<T>(
        string input,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both) => (T)Parse(input, typeof(T), null, culture, trimmingOptions);

    public static bool TryParse<T>(
        string input,
        out T value,
        T defaultValue = default!,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var r = TryParse(input, typeof(T), out var result, defaultValue, culture, trimmingOptions);

        value = (T)(result ?? defaultValue)!;

        return r;
    }

    #region type argument
    public static object? Parse(
        object? value,
        Type type,
        object? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        if (value is null || value == DBNull.Value)
        {
            return defaultValue;
        }

        return Parse($"{value}", type, defaultValue, culture, trimmingOptions);
    }
    #endregion

    public static object Parse(
        string input,
        Type type,
        object? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParse(input, type, out var value, defaultValue, culture, trimmingOptions), input, value!);
    }

    public static bool TryParse(
        string input,
        Type type,
        out object? value,
        object? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        ArgumentNullException.ThrowIfNull(type);

        input = Trim(input, trimmingOptions);
        var empty = string.IsNullOrEmpty(input);

        if (empty)
        {
            value = defaultValue;

            return false;
        }

        if (type == typeof(string))
        {
            value = empty
                ? defaultValue
                : input;

            return !empty;
        }

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (!type.IsValueType)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionNotSupported, typeof(string), type));
        }

        // handle the null default value case (empty/null value and null default value has been already handled)
        defaultValue ??= type.GetDefaultValue();
        culture ??= CultureInfo.InvariantCulture;

        Debug.Assert(defaultValue != null, $"{nameof(defaultValue)} != null");

        // hanle types without a type code first
        if (type.IsEnum)
        {
            return TryParseEnum(type, input, out value, (Enum)defaultValue, trimmingOptions);
        }

        bool r;

        // types without a type code
        if (type == typeof(Guid))
        {
            r = TryParseGuid(input, out var output, (Guid)defaultValue, trimmingOptions);
            value = output;
            return r;
        }
        else if (type == typeof(DateOnly))
        {
            r = TryParseDate(input, out var output, (DateOnly)defaultValue, culture, trimmingOptions);
            value = output;
            return r;
        }
        else if (type == typeof(TimeOnly))
        {
            r = TryParseTime(input, out var output, (TimeOnly)defaultValue, culture, trimmingOptions);
            value = output;
            return r;
        }
        else if (type == typeof(DateTimeOffset))
        {
            r = TryParseDateTimeOffset(input, out var output, (DateTimeOffset)defaultValue, culture, trimmingOptions);
            value = output;
            return r;
        }
        else if (type == typeof(TimeSpan))
        {
            r = TryParseTimeSpan(input, out var output, (TimeSpan)defaultValue, culture, trimmingOptions);
            value = output;
            return r;
        }

        var typeCode = Type.GetTypeCode(type);

        switch (typeCode)
        {
            // most probable conversions first
            case TypeCode.Int32:
                {
                    r = TryParseInt(input, out var output, (int)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.Int64:
                {
                    r = TryParseLong(input, out var output, (long)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Double:
                {
                    r = TryParseDouble(input, out var output, (double)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Decimal:
                {
                    r = TryParseDecimal(input, out var output, (decimal)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.DateTime:
                {
                    r = TryParseDateTime(input, out var output, (DateTime)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Boolean:
                {
                    r = TryParseBoolean(input, out var output, (bool)defaultValue);
                    value = output;
                    break;
                }

            case TypeCode.Single:
                {
                    r = TryParseFloat(input, out var output, (float)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Int16:
                {
                    r = TryParseShort(input, out var output, (short)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Byte:
                {
                    r = TryParseByte(input, out var output, (byte)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.SByte:
                {
                    r = TryParseSByte(input, out var output, (sbyte)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.UInt16:
                {
                    r = TryParseUShort(input, out var output, (ushort)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.UInt32:
                {
                    r = TryParseUInt(input, out var output, (uint)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.UInt64:
                {
                    r = TryParseULong(input, out var output, (ulong)defaultValue, culture, trimmingOptions);
                    value = output;
                    break;
                }

            case TypeCode.Char:
                {
                    r = TryParseChar(input, out var output, (char)defaultValue, trimmingOptions);
                    value = output;
                    break;
                }

            // unreacheable
            default:
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionNotSupported, typeof(string), type), nameof(type));
        }

        return r;
    }
    #endregion

    #region known type
    #region boolean
    public static bool ParseBoolean(
        string input,
        bool defaultValue = false)
    {
        return TryOrThrow(TryParseBoolean(input, out bool value, defaultValue), input, value);
    }

    public static bool TryParseBoolean(
        string input,
        out bool value,
        bool defaultValue = false)
    {
        if (bool.TryParse(input, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static bool? ParseNullableBoolean(string input)
    {
        return TryOrThrow(TryParseNullableBoolean(input, out var value), input, value);
    }

    public static bool TryParseNullableBoolean(
        string input,
        out bool? value)
    {
        if (TryParseBoolean(input, out bool parsedValue))
        {
            value = parsedValue;

            return true;
        }

        value = default;

        return false;
    }
    #endregion

    #region char
    public static char ParseChar(
        string input,
        char defaultValue = '\0',
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseChar(input, out var value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseChar(
    string input,
    out char value,
    char defaultValue = '\0',
    TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue;

            return false;
        }

        return char.TryParse(trimmed, out value);
    }

    public static char? ParseNullableChar(string input, TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableChar(input, out var value, trimmingOptions), input, value);
    }

    public static bool TryParseNullableChar(
        string input,
        out char? value,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        var r = char.TryParse(trimmed, out char v);

        value = v;

        return r;
    }
    #endregion

    #region enum
    public static T ParseEnum<T>(string input, T defaultValue = default, TrimmingOptions trimmingOptions = TrimmingOptions.Both) where T : struct
    {
        return TryOrThrow(TryParseEnum(input, out T value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseEnum<T>(
        string input,
        out T value,
        T defaultValue = default!,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (EnumHelper.TryParse(trimmed, true, out value!))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static T? ParseNullableEnum<T>(
        string input,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both) where T : struct
    {
        return TryOrThrow(TryParseNullableEnum(input, out T? value, trimmingOptions), input, value);
    }

    public static bool TryParseNullableEnum<T>(
        string input,
        out T? value,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both) where T : struct
    {
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = null;

            return true;
        }

        if (EnumHelper.TryParse(trimmed, true, out value))
        {
            return true;
        }

        value = null;

        return false;
    }

    public static object ParseEnum(
        Type enumType,
        string input,
        object defaultValue,
        TrimmingOptions trimmingOptions)
    {
        return TryOrThrow(TryParseEnum(enumType, input, out var value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseEnum(
        Type enumType,
        string input,
        out object value,
        object defaultValue,
        TrimmingOptions trimmingOptions)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue;

            return false;
        }

        return EnumHelper.TryParse(enumType, trimmed, true, out value!);
    }
    #endregion

    #region guid
    public static Guid ParseGuid(
        string input,
        Guid defaultValue,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseGuid(input, out var value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseGuid(
        string input,
        out Guid value,
        Guid? defaultValue,
        TrimmingOptions trimmingOptions)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? Guid.Empty;

            return false;
        }

        return Guid.TryParse(trimmed, out value);

    }

    public static Guid? ParseNullableGuid(string input, TrimmingOptions trimmingOptions)
    {
        return TryOrThrow(TryParseNullableGuid(input, out var value, trimmingOptions), input, value);
    }

    public static bool TryParseNullableGuid(
        string input,
        out Guid? value,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (Guid.TryParse(trimmed, out Guid guidValue))
        {
            value = guidValue;

            return true;
        }

        return false;
    }
    #endregion

    #region dateonly
    public static DateOnly ParseDate(
        string input,
        DateOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseDate(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseDate(
        string input,
        out DateOnly value,
        DateOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        culture ??= CultureInfo.InvariantCulture;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? DateOnly.MinValue;

            return false;
        }

        return DateOnly.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateOnly.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value) ||
            DateOnly.TryParse(trimmed, culture, DateTimeStyles, out value);
    }

    public static DateOnly? ParseNullableDate(
        string input,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableDate(input, out var value, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableDate(
        string input,
        out DateOnly? value,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (!string.IsNullOrEmpty(trimmed) &&
            TryParseDate(input, out var dtValue, null, culture, trimmingOptions))
        {
            value = dtValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region datetime
    public static DateTime ParseDateTime(
        string input,
        DateTime? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseDateTime(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseDateTime(
        string input,
        out DateTime value,
        DateTime? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? DateTime.MinValue;

        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        return
            DateTime.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateTime.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value) ||
            DateTime.TryParse(trimmed, culture, DateTimeStyles, out value);
    }

    public static DateTime? ParseNullableDateTime(
        string input,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableDateTime(input, out var value, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableDateTime(
        string input,
        out DateTime? value,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (TryParseDateTime(input, out var dtValue, null, culture, trimmingOptions))
        {
            value = dtValue;

            return true;
        }

        return false;
    }
    #endregion

    #region datetime offset
    public static DateTimeOffset ParseDateTimeOffset(
        string input,
        DateTimeOffset? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseDateTimeOffset(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseDateTimeOffset(
        string input,
        out DateTimeOffset value,
        DateTimeOffset? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? DateTimeOffset.MinValue;
        culture ??= CultureInfo.InvariantCulture;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        return
            DateTimeOffset.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateTimeOffset.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value) ||
            DateTimeOffset.TryParse(trimmed, culture, DateTimeStyles, out value);
    }

    public static DateTimeOffset? ParseNullableDateTimeOffset(
        string input,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableDateTimeOffset(input, out var value, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableDateTimeOffset(
        string input,
        out DateTimeOffset? value,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (TryParseDateTimeOffset(trimmed, out var parsedValue, null, culture, trimmingOptions))
        {
            value = parsedValue;

            return true;
        }

        return false;
    }
    #endregion

    #region timespan
    public static TimeSpan ParseTimeSpan(
        string input,
        TimeSpan? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseTimeSpan(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseTimeSpan(
        string input,
        out TimeSpan value,
        TimeSpan? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? TimeSpan.MinValue;
        culture ??= CultureInfo.InvariantCulture;
        
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        return
            TimeSpan.TryParseExact(trimmed, timeFormats, culture, TimeStyles, out value) ||
            TimeSpan.TryParseExact(trimmed, "G", culture, TimeStyles, out value) ||
            TimeSpan.TryParse(trimmed, culture, out value);
    }


    public static TimeSpan? ParseNullableTimeSpan(
        string input,
        TimeSpan? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableTimeSpan(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableTimeSpan(
        string input,
        out TimeSpan? value,
        TimeSpan? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? TimeSpan.MinValue;
        culture ??= CultureInfo.InvariantCulture;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (TimeSpan.TryParse(trimmed, culture, out var parsedValue))
        {
            value = parsedValue;

            return true;
        }

        return false;
    }
    #endregion

    #region timeonly
    public static TimeOnly ParseTime(
        string input,
        TimeOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseTime(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseTime(
        string input,
        out TimeOnly value,
        TimeOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? TimeOnly.MinValue;
        culture ??= CultureInfo.InvariantCulture;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        return
            TimeOnly.TryParseExact(trimmed, timeFormats, culture, DateTimeStyles, out value) ||
            TimeOnly.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value) ||
            TimeOnly.TryParse(trimmed, culture, out value);
    }


    public static TimeOnly? ParseNullableTime(
        string input,
        TimeOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableTime(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableTime(
        string input,
        out TimeOnly? value,
        TimeOnly? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? TimeOnly.MinValue;
        culture ??= CultureInfo.InvariantCulture;
        
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (TimeOnly.TryParse(trimmed, culture, out var parsedValue))
        {
            value = parsedValue;
            
            return true;
        }

        return false;
    }
    #endregion

    #region sbyte
    public static sbyte ParseSByte(
        string input,
        sbyte? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseSByte(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseSByte(
        string input,
        out sbyte value,
        sbyte? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = defaultValue ?? sbyte.MinValue;
        culture ??= CultureInfo.InvariantCulture;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        return sbyte.TryParse(trimmed, IntegerNumberStyles, culture, out value);
    }

    public static sbyte? ParseNullableSByte(
        string input,
        sbyte? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableSByte(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableSByte(
        string input,
        out sbyte? value,
        sbyte? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? sbyte.MinValue;
            return true;
        }

        if (sbyte.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
    }
    #endregion

    #region byte
    public static byte ParseByte(
        string input,
        byte? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseByte(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseByte(
        string input,
        out byte value,
        byte? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? byte.MinValue;
        }
        else if (!byte.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
    }

    public static byte? ParseNullableByte(
        string input,
        byte? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableByte(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableByte(
        string input,
        out byte? value,
        byte? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? byte.MinValue;
            return true;
        }

        if (byte.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
    }
    #endregion

    #region short (int16)
    public static short ParseShort(
        string input,
        short? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseShort(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseShort(
        string input,
        out short value,
        short? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? short.MinValue;
        }
        else if (!short.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
    }

    public static short? ParseNullableShort(
        string input,
        short? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableShort(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableShort(
        string input,
        out short? value,
        short? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? short.MinValue;
            return true;
        }

        if (short.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
    }
    #endregion

    #region ushort
    public static ushort ParseUShort(
        string input,
        ushort? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseUShort(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseUShort(
        string input,
        out ushort value,
        ushort? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? ushort.MinValue;
        }
        else if (!ushort.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
    }

    public static ushort? ParseNullableUShort(
        string input,
        ushort? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableUShort(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableUShort(
        string input,
        out ushort? value,
        ushort? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? ushort.MinValue;
            return true;
        }

        if (ushort.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
    }
    #endregion

    #region int
    public static int ParseInt(
        string input,
        int? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseInt(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseInt(
        string input,
        out int value,
        int? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (int.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static int? ParseNullableInt(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableInt(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableInt(
        string input,
        out int? value,
        IFormatProvider? culture = null)
    {
        if (TryParseInt(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }
        
        value = null;

        return false;
    }
    #endregion

    #region uint
    public static uint ParseUInt(
        string input,
        uint? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseUInt(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseUInt(
        string input,
        out uint value,
        uint? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (uint.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static uint? ParseNullableUInt(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableUInt(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableUInt(
        string input,
        out uint? value,
        IFormatProvider? culture = null)
    {
        if (TryParseUInt(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }
        
        value = null;

        return false;
    }
    #endregion

    #region long
    public static long ParseLong(
        string input,
        long? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseLong(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseLong(
        string input,
        out long value,
        long? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? long.MinValue;

            return false;
        }

        return long.TryParse(trimmed, IntegerNumberStyles, culture, out value);
    }

    public static long? ParseNullableLong(
        string input,
        long? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableLong(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableLong(
        string input,
        out long? value,
        long? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? long.MinValue;

            return false;
        }

        var r = long.TryParse(trimmed, IntegerNumberStyles, culture, out var v);

        value = v;

        return r;
    }
    #endregion

    #region ulong
    public static ulong ParseULong(
        string input,
        ulong? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseULong(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseULong(
        string input,
        out ulong value,
        ulong? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? ulong.MinValue;

            return false;
        }

        return ulong.TryParse(trimmed, IntegerNumberStyles, culture, out value);
    }

    public static ulong? ParseNullableULong(
        string input,
        ulong? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableULong(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableULong(
        string input,
        out ulong? value,
        ulong? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? ulong.MinValue;

            return false;
        }

        var r = ulong.TryParse(trimmed, IntegerNumberStyles, culture, out var v);

        value = v;

        return r;
    }
    #endregion

    #region float
    public static float ParseFloat(
        string input,
        float? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseFloat(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseFloat(
        string input,
        out float value,
        float? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? float.MinValue;

            return false;
        }

        return float.TryParse(trimmed, FloatingPointNumberStyles, culture, out value);
    }

    public static float? ParseNullableFloat(
        string input,
        float? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableFloat(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableFloat(
        string input,
        out float? value,
        float? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? float.MinValue;

            return false;
        }

        var r = float.TryParse(trimmed, FloatingPointNumberStyles, culture, out var v);

        value = v;

        return r;
    }
    #endregion

    #region double
    public static double ParseDouble(
        string input,
        double? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseDouble(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseDouble(
        string input,
        out double value,
        double? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? double.MinValue;

            return false;
        }

        return double.TryParse(trimmed, FloatingPointNumberStyles, culture, out value);
    }

    public static double? ParseNullableDouble(
        string input,
        double? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableDouble(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableDouble(
        string input,
        out double? value,
        double? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? double.MinValue;

            return false;
        }

        var r = double.TryParse(trimmed, FloatingPointNumberStyles, culture, out var v);

        value = v;

        return r;
    }
    #endregion

    #region Decimal
    public static decimal ParseDecimal(
        string input,
        decimal? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseDecimal(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseDecimal(
        string input,
        out decimal value,
        decimal? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? decimal.MinValue;

            return false;
        }

        return decimal.TryParse(trimmed, FloatingPointNumberStyles, culture, out value);
    }

    public static decimal? ParseNullableDecimal(
        string input,
        decimal? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableDecimal(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableDecimal(
        string input,
        out decimal? value,
        decimal? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (decimal.TryParse(trimmed, FloatingPointNumberStyles, culture, out var v))
        {
            value = v;
            return true;
        }

        value = defaultValue;

        return false;
    }
    #endregion

        #region culture info
    public static CultureInfo ParseCultureInfo(string? name)
    {
        CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        try
        {
            cultureInfo = CultureInfo.CreateSpecificCulture(name!);
        }
        catch
        {
        }

        return cultureInfo;
    }
    #endregion

    #region Encoding
    public static Encoding ParseEncoding(string encodingName)
    {
        var encoding = Encoding.Default;

        try
        {
            encoding = Encoding.GetEncoding(encodingName);
        }
        catch
        {
        }

        return encoding;
    }
    #endregion
    #endregion
    #endregion
}