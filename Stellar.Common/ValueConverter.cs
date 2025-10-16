using Stellar.Common.Resources;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Stellar.Common;

/// <summary>
/// Common value conversion wrapping intrinsic converters
/// with extended functionality.
/// </summary>
public static class ValueConverter
{
    #region styles
    public static NumberStyles IntegerNumberStyles { get; set; } = NumberStyles.Integer | NumberStyles.AllowThousands;
    public static NumberStyles FloatingPointNumberStyles { get; set; } = NumberStyles.Float | NumberStyles.AllowThousands;
    public static NumberStyles DecimalNumberStyles { get; set; } = FloatingPointNumberStyles | NumberStyles.AllowParentheses | NumberStyles.AllowCurrencySymbol;
    public static DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.AllowWhiteSpaces;
    public static TimeSpanStyles TimeStyles { get; set; } = TimeSpanStyles.None;
    #endregion

    #region formats
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

    private static string[] timeFormats =
    [
        "HHmmss",
        "HHmm"
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
    public static string Trim(string value, TrimmingOptions options, char[]? chars = null)
    {
        if ((options & TrimmingOptions.Both) == TrimmingOptions.Both)
        {
            value = value.Trim(chars);
        }
        else if ((options & TrimmingOptions.End) == TrimmingOptions.End)
        {
            value = value.TrimEnd(chars);
        }
        else if ((options & TrimmingOptions.Start) == TrimmingOptions.Start)
        {
            value = value.TrimStart(chars);
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

        value = (T)result!;

        return r;
    }

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

        if (input is null)
        {
            value = defaultValue;

            return false;
        }
        
        input = Trim(input, trimmingOptions);

        if (type == typeof(string))
        {
            value = input;

            return true;
        }

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (!type.IsValueType)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionNotSupported, typeof(string), type));
        }

        // handle the null default value case (empty/null value and null default value has been already handled)
        defaultValue ??= type.GetDefaultValue();

        Debug.Assert(defaultValue != null, $"{nameof(defaultValue)} != null");

        // hanle types without a type code first
        if (type.IsEnum)
        {
            return TryParseEnum(type, input, out value, (Enum)defaultValue, trimmingOptions);
        }

        culture ??= CultureInfo.InvariantCulture;
        
        bool r;

        // types without a type code
        if (type == typeof(Guid))
        {
            r = TryParseGuid(input, out var output, (Guid)defaultValue);
            value = output;
            return r;
        }
        else if (type == typeof(DateOnly))
        {
            r = TryParseDate(input, out var output, (DateOnly)defaultValue, culture);
            value = output;
            return r;
        }
        else if (type == typeof(TimeOnly))
        {
            r = TryParseTime(input, out var output, (TimeOnly)defaultValue, culture);
            value = output;
            return r;
        }
        else if (type == typeof(DateTimeOffset))
        {
            r = TryParseDateTimeOffset(input, out var output, (DateTimeOffset)defaultValue, culture);
            value = output;
            return r;
        }
        else if (type == typeof(TimeSpan))
        {
            r = TryParseTimeSpan(input, out var output, (TimeSpan)defaultValue, culture);
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
                    r = TryParseLong(input, out var output, (long)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.Double:
                {
                    r = TryParseDouble(input, out var output, (double)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.Decimal:
                {
                    r = TryParseDecimal(input, out var output, (decimal)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.DateTime:
                {
                    r = TryParseDateTime(input, out var output, (DateTime)defaultValue, culture);
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
                    r = TryParseFloat(input, out var output, (float)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.Int16:
                {
                    r = TryParseShort(input, out var output, (short)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.Byte:
                {
                    r = TryParseByte(input, out var output, (byte)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.SByte:
                {
                    r = TryParseSByte(input, out var output, (sbyte)defaultValue, culture);
                    value = output;
                    break;
                }

            case TypeCode.UInt16:
                {
                    r = TryParseUnsignedShort(input, out var output, (ushort)defaultValue, culture);
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
                    r = TryParseULong(input, out var output, (ulong)defaultValue, culture);
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
    #region sbyte
    public static sbyte ParseSByte(
        string input,
        sbyte defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseSByte(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseSByte(
        string input,
        out sbyte value,
        sbyte defaultValue = default,
        IFormatProvider? culture = null)
    {
        if (sbyte.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static sbyte? ParseNullableSByte(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableSByte(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableSByte(
        string input,
        out sbyte? value,
        IFormatProvider? culture = null)
    {
        if (TryParseSByte(input, out var parsedValue, default, culture ?? CultureInfo.InvariantCulture))
        {
            value = parsedValue;
            
            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region byte
    public static byte ParseByte(
        string input,
        byte defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseByte(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseByte(
        string input,
        out byte value,
        byte defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (byte.TryParse(input, IntegerNumberStyles, culture, out value))
        {
            return true;
        }

        value = defaultValue;
        
        return false;
    }

    public static byte? ParseNullableByte(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableByte(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableByte(
        string input,
        out byte? value,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (TryParseByte(input, out var parsedValue, default, culture))
        {
            value = parsedValue;
            
            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region short
    public static short ParseShort(
        string input,
        short defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseShort(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseShort(
        string input,
        out short value,
        short defaultValue = default,
        IFormatProvider? culture = null)
    {
        if (short.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static short? ParseNullableShort(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableShort(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableShort(
        string input,
        out short? value,
        IFormatProvider? culture = null)
    {
        if (TryParseShort(input, out var parsedValue, default, culture ?? CultureInfo.InvariantCulture))
        {
            value = parsedValue;
            
            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region ushort
    public static ushort ParseUnsignedShort(
        string input,
        ushort defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseUnsignedShort(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseUnsignedShort(
        string input,
        out ushort value,
        ushort defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (ushort.TryParse(input, IntegerNumberStyles, culture, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static ushort? ParseNullableUnsignedShort(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableUnsignedShort(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableUnsignedShort(
        string input,
        out ushort? value,
        IFormatProvider? culture = null)
    {
        if (TryParseUnsignedShort(input, out var parsedValue, default, culture ?? CultureInfo.InvariantCulture))
        {
            value = parsedValue;
            
            return true;
        }

        value = null;

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
        long? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseLong(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseLong(
        string input,
        out long value,
        long? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (long.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static long? ParseNullableLong(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableLong(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableLong(
        string input,
        out long? value,
        IFormatProvider? culture = null)
    {
        if (TryParseLong(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region ulong
    public static ulong ParseULong(
        string input,
        ulong? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseULong(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseULong(
        string input,
        out ulong value,
        ulong? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (ulong.TryParse(input, IntegerNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static ulong? ParseNullableULong(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableULong(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableULong(
        string input,
        out ulong? value,
        IFormatProvider? culture = null)
    {
        if (TryParseULong(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region float
    public static float ParseFloat(
        string input,
        float? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseFloat(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseFloat(
        string input,
        out float value,
        float? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (float.TryParse(input, FloatingPointNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static float? ParseNullableFloat(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableFloat(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableFloat(
        string input,
        out float? value,
        IFormatProvider? culture = null)
    {
        if (TryParseFloat(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region double
    public static double ParseDouble(
        string input,
        double? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseDouble(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseDouble(
        string input,
        out double value,
        double? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (double.TryParse(input, FloatingPointNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static double? ParseNullableDouble(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableDouble(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableDouble(
        string input,
        out double? value,
        IFormatProvider? culture = null)
    {
        if (TryParseDouble(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region decimal
    public static decimal ParseDecimal(
        string input,
        decimal? defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseDecimal(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseDecimal(
        string input,
        out decimal value,
        decimal? defaultValue = null,
        IFormatProvider? culture = null)
    {
        if (decimal.TryParse(input, DecimalNumberStyles, culture ?? CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static decimal? ParseNullableDecimal(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableDecimal(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableDecimal(
        string input,
        out decimal? value,
        IFormatProvider? culture = null)
    {
        if (TryParseDecimal(input, out var v, culture: culture))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region bool
    public static bool ParseBoolean(
        string input,
        bool? defaultValue = default)
    {
        return TryOrThrow(TryParseBoolean(input, out var value, defaultValue), input, value);
    }

    public static bool TryParseBoolean(
        string input,
        out bool value,
        bool? defaultValue = null)
    {
        if (bool.TryParse(input, out value))
        {
            return true;
        }

        value = defaultValue ?? default;

        return false;
    }

    public static bool? ParseNullableBoolean(
        string input)
    {
        return TryOrThrow(TryParseNullableBoolean(input, out var value), input, value);
    }

    public static bool TryParseNullableBoolean(
        string input,
        out bool? value)
    {
        if (TryParseBoolean(input, out var v))
        {
            value = v;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region char
    public static char ParseChar(
        string input,
        char defaultValue = default,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseChar(input, out var value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseChar(
    string input,
    out char value,
    char defaultValue = default,
    TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (char.TryParse(trimmed, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static char? ParseNullableChar(
        string input,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableChar(input, out var value, trimmingOptions), input, value);
    }

    public static bool TryParseNullableChar(
        string input,
        out char? value,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        if (char.TryParse(trimmed, out char parsedValue))
        {
            value = parsedValue;
            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region enum
    public static T ParseEnum<T>(
        string input,
        T defaultValue = default,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both) where T : struct
    {
        return TryOrThrow(TryParseEnum(input, out T value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseEnum<T>(
        string input,
        out T value,
        T defaultValue = default!,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        if (EnumHelper.TryParse(Trim(input, trimmingOptions), true, out value!))
        {
            return true;
        }

        value = defaultValue;

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
        if (EnumHelper.TryParse(enumType, Trim(input, trimmingOptions), true, out value!))
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
        return EnumHelper.TryParse(Trim(input, trimmingOptions), true, out value);
    }
    #endregion

    #region guid
    public static Guid ParseGuid(
        string input,
        Guid defaultValue = default)
    {
        return TryOrThrow(TryParseGuid(input, out var value, defaultValue), input, value);
    }

    public static bool TryParseGuid(
        string input,
        out Guid value,
        Guid defaultValue = default)
    {
        if (Guid.TryParse(input, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static Guid? ParseNullableGuid(
        string input)
    {
        return TryOrThrow(TryParseNullableGuid(input, out var value), input, value);
    }

    public static bool TryParseNullableGuid(
        string input,
        out Guid? value)
    {
        if (Guid.TryParse(input, out Guid guidValue))
        {
            value = guidValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region date
    public static DateOnly ParseDate(
        string input,
        DateOnly defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseDate(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseDate(
        string input,
        out DateOnly value,
        DateOnly defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (DateOnly.TryParseExact(input, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateOnly.TryParseExact(input, "G", culture, DateTimeStyles, out value) ||
            DateOnly.TryParse(input, culture, DateTimeStyles, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static DateOnly? ParseNullableDate(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableDate(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableDate(
        string input,
        out DateOnly? value,
        IFormatProvider? culture = null)
    {
        if (TryParseDate(input, out var parsedValue, default, culture))
        {
            value = parsedValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region time
    public static TimeOnly ParseTime(
        string input,
        TimeOnly defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseTime(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseTime(
        string input,
        out TimeOnly value,
        TimeOnly defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (TimeOnly.TryParseExact(input, timeFormats, culture, DateTimeStyles, out value) ||
            TimeOnly.TryParseExact(input, "G", culture, DateTimeStyles, out value) ||
            TimeOnly.TryParse(input, culture, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static TimeOnly? ParseNullableTime(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableTime(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableTime(
        string input,
        out TimeOnly? value,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (TryParseTime(input, out var parsedValue, culture: culture))
        {
            value = parsedValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region datetime
    public static DateTime ParseDateTime(
        string input,
        DateTime defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseDateTime(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseDateTime(
        string input,
        out DateTime value,
        DateTime defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (DateTime.TryParseExact(input, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateTime.TryParseExact(input, "G", culture, DateTimeStyles, out value) ||
            DateTime.TryParse(input, culture, DateTimeStyles, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static DateTime? ParseNullableDateTime(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableDateTime(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableDateTime(
        string input,
        out DateTime? value,
        IFormatProvider? culture = null)
    {
        if (TryParseDateTime(input, out var dtValue, default, culture))
        {
            value = dtValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region datetime offset
    public static DateTimeOffset ParseDateTimeOffset(
        string input,
        DateTimeOffset defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseDateTimeOffset(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseDateTimeOffset(
        string input,
        out DateTimeOffset value,
        DateTimeOffset defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (DateTimeOffset.TryParseExact(input, datetimeFormats, culture, DateTimeStyles, out value) ||
            DateTimeOffset.TryParseExact(input, "G", culture, DateTimeStyles, out value) ||
            DateTimeOffset.TryParse(input, culture, DateTimeStyles, out value))
        {
            return true;
        }
        
        value = defaultValue;
        
        return false;
    }

    public static DateTimeOffset? ParseNullableDateTimeOffset(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableDateTimeOffset(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableDateTimeOffset(
        string input,
        out DateTimeOffset? value,
        IFormatProvider? culture = null)
    {
        if (TryParseDateTimeOffset(input, out var parsedValue, default, culture))
        {
            value = parsedValue;

            return true;
        }

        value = null;

        return false;
    }
    #endregion

    #region timespan
    public static TimeSpan ParseTimeSpan(
        string input,
        TimeSpan defaultValue = default,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseTimeSpan(input, out var value, defaultValue, culture), input, value);
    }

    public static bool TryParseTimeSpan(
        string input,
        out TimeSpan value,
        TimeSpan defaultValue = default,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (TimeSpan.TryParseExact(input, timeFormats, culture, TimeStyles, out value) ||
            TimeSpan.TryParseExact(input, "G", culture, TimeStyles, out value) ||
            TimeSpan.TryParse(input, culture, out value))
        {
            return true;
        }

        value = defaultValue;

        return false;
    }

    public static TimeSpan? ParseNullableTimeSpan(
        string input,
        IFormatProvider? culture = null)
    {
        return TryOrThrow(TryParseNullableTimeSpan(input, out var value, culture), input, value);
    }

    public static bool TryParseNullableTimeSpan(
        string input,
        out TimeSpan? value,
        IFormatProvider? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        if (TryParseTimeSpan(input, out var parsedValue, default, culture))
        {
            value = parsedValue;

            return true;
        }

        value = null;

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
}