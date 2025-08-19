using Stellar.Common.Resources;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Stellar.Common;

/// <summary>
/// Common ETL value conversions.
/// </summary>
public static class ValueConverter
{
    #region styles
    public static NumberStyles IntegerNumberStyles { get; set; } = NumberStyles.Integer;
    public static NumberStyles FloatingPointNumberStyles { get; set; } = NumberStyles.Float;
    public static DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;
    public static TimeSpanStyles TimeStyles { get; set; } = TimeSpanStyles.None;
    #endregion

    #region datetime formats (initialzed with JavaScript formats)
    private static string[] datetimeFormats =
    [
        // JavaScript
        "ddd MMM d HH:mm:ss 'GMT'zzz yyyy",
        "ddd MMM d HH:mm:ss 'UTC'zzz yyyy",
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
    private static string[] timeFormats = [];

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
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        if (value is null || Convert.IsDBNull(value))
        {
            return emptyString;
        }

        culture ??= CultureInfo.InvariantCulture;

        return value switch
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

            _ => Trim(string.IsNullOrWhiteSpace(format)
                    ? $"{value}".ToString(culture)
                    : string.Format(culture, $"{{0:{format}}}", value),
                trimmingOptions)
        };
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
        if (TryParse(input, typeof(T), out var result, defaultValue, culture, trimmingOptions))
        {
            value = (T)result!;

            return true;
        }

        value = default!;

        return false;
    }

    #region type argument
    public static object? Parse(
        object? value,
        Type type,
        object? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        if (value == DBNull.Value)
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

        value = null;
        input = Trim(input, trimmingOptions);

        if (type == typeof(string))
        {
            value = string.IsNullOrEmpty(input)
                ? defaultValue
                : input;

            return true;
        }

        var underlyingType = Nullable.GetUnderlyingType(type);

        if (underlyingType != null)
        {
            if (string.IsNullOrEmpty(input))
            {
                value = defaultValue;
                return true;
            }

            type = underlyingType;
        }
        else if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (!type.IsValueType)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionNotSupported, typeof(string), type), nameof(type));
        }

        // handle the null default value case (empty/null value and null default value has been already handled)
        defaultValue ??= type.GetDefaultValue();

        Debug.Assert(defaultValue != null, nameof(defaultValue) + " != null");

        culture ??= CultureInfo.InvariantCulture;

        // hanle types without a type code first
        if (type.IsEnum)
        {
            return TryParseEnum(type, input, out value, (Enum)defaultValue, trimmingOptions);
        }

        if (type == typeof(Guid))
        {
            if (!TryParseGuid(input, out var output, (Guid)defaultValue, trimmingOptions))
            {
                return false;
            }

            value = output;
            return true;
        }

        if (type == typeof(DateOnly))
        {
            if (!TryParseDate(input, out var output, (DateOnly)defaultValue, culture, trimmingOptions))
            {
                return false;
            }

            value = output;
            return true;
        }

        if (type == typeof(TimeOnly))
        {
            if (!TryParseTime(input, out var output, (TimeOnly)defaultValue, culture, trimmingOptions))
            {
                return false;
            }

            value = output;
            return true;
        }

        var typeCode = Type.GetTypeCode(type);

        switch (typeCode)
        {
            // most probable conversions first
            case TypeCode.Int32:
                {
                    if (TryParseInt(input, out var output, (int)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Int64:
                {
                    if (TryParseLong(input, out var output, (long)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Double:
                {
                    if (TryParseDouble(input, out var output, (double)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Decimal:
                {
                    if (TryParseDecimal(input, out var output, (decimal)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.DateTime:
                {
                    if (TryParseDateTime(input, out var output, (DateTime)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Boolean:
                {
                    if (TryParseBoolean(input, out var output, (bool)defaultValue, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Single:
                {
                    if (TryParseFloat(input, out var output, (float)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Int16:
                {
                    if (TryParseShort(input, out var output, (short)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Byte:
                {
                    if (TryParseByte(input, out var output, (byte)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.SByte:
                {
                    if (TryParseSByte(input, out var output, (sbyte)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.UInt16:
                {
                    if (TryParseUShort(input, out var output, (ushort)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.UInt32:
                {
                    if (TryParseUInt(input, out var output, (uint)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.UInt64:
                {
                    if (TryParseULong(input, out var output, (ulong)defaultValue, culture, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            case TypeCode.Char:
                {
                    if (TryParseChar(input, out var output, (char)defaultValue, trimmingOptions))
                    {
                        value = output;

                        return true;
                    }

                    break;
                }

            // handled (unreacheable)
            case TypeCode.String:
                {
                    value = input;
                    return true;
                }

            // unsupported (unreachable)
            case TypeCode.Object:
            case TypeCode.Empty:
                goto default;

            default:
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Exceptions.ConversionNotSupported, typeof(string), type), nameof(type));
        }

        return false;
    }
    #endregion

    #region known type
    #region boolean
    public static bool ParseBoolean(
        string input,
        bool defaultValue = false,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseBoolean(input, out bool value, defaultValue, trimmingOptions), input, value);
    }

    public static bool TryParseBoolean(
        string input,
        out bool value,
        bool defaultValue = false,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        input = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(input))
        {
            value = defaultValue;
            return true;
        }

        if (bool.TryParse(input, out value))
        {
            return true;
        }

        if (int.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var intValue))
        {
            if (intValue < 0 || 1 < intValue)
            {
                value = defaultValue;
                return false;
            }

            value = intValue == 1;
            return true;
        }

        return false;
    }

    public static bool? ParseNullableBoolean(
        string input,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableBoolean(input, out var value, trimmingOptions), input, value);
    }

    public static bool TryParseNullableBoolean(
        string input,
        out bool? value,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        input = Trim(input, trimmingOptions);
        value = null;

        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        if (bool.TryParse(input, out bool boolValue))
        {
            value = boolValue;
            return true;
        }

        if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var intValue))
        {
            if (intValue < 0 || 1 < intValue)
            {
                return false;
            }

            value = intValue == 1;
            return true;
        }

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

        if (!string.IsNullOrEmpty(trimmed) && char.TryParse(trimmed, out value))
        {
            return true;
        }

        value = defaultValue;
        return false;
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

        if (char.TryParse(trimmed, out char charValue))
        {
            value = charValue;
            return true;
        }

        value = null;
        return false;
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

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue;
            return true;
        }

        return EnumHelper.TryParse(input, true, out value!);
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

        if (EnumHelper.TryParse(trimmed, true, out T tValue))
        {
            value = tValue;
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
            return true;
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
        value = defaultValue ?? Guid.Empty;

        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            return true;
        }

        if (Guid.TryParse(trimmed, out Guid guidValue))
        {
            value = guidValue;
            return true;
        }

        return false;
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
            return true;
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
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (!string.IsNullOrEmpty(trimmed))
        {
            if (datetimeFormats.Length > 0 &&
                DateOnly.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateOnly.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateOnly.TryParse(trimmed, culture, DateTimeStyles, out value))
            {
                return true;
            }
        }

        value = defaultValue ?? DateOnly.MinValue;
        
        return false;
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
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (!string.IsNullOrEmpty(trimmed))
        {
            if (datetimeFormats.Length > 0 &&
                DateTime.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateTime.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateTime.TryParse(trimmed, culture, DateTimeStyles, out value))
            {
                return true;
            }
        }

        value = defaultValue ?? DateTime.MinValue;
        
        return false;
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
        var trimmed = Trim(input, trimmingOptions);

        if (!string.IsNullOrEmpty(trimmed) &&
            TryParseDateTime(input, out var dtValue, null, culture, trimmingOptions))
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
        var trimmed = Trim(input, trimmingOptions);

        if (!string.IsNullOrEmpty(trimmed))
        {
            culture ??= CultureInfo.InvariantCulture;

            if (datetimeFormats.Length != 0 &&
                DateTimeOffset.TryParseExact(trimmed, datetimeFormats, culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateTimeOffset.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value))
            {
                return true;
            }

            if (DateTimeOffset.TryParse(trimmed, culture, DateTimeStyles, out value))
            {
                return true;
            }
        }

        value = defaultValue ?? DateTimeOffset.MinValue;
        return false;
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
            return true;
        }

        if (TryParseDateTimeOffset(trimmed, out var parsedValue, DateTimeOffset.MinValue, culture, trimmingOptions))
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
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? TimeSpan.MinValue;
            return true;
        }

        culture ??= CultureInfo.InvariantCulture;

        if (!string.IsNullOrEmpty(trimmed))
        {
            if (timeFormats.Length > 0 &&
                TimeSpan.TryParseExact(trimmed, timeFormats, culture, TimeStyles, out value))
            {
                return true;
            }

            if (TimeSpan.TryParseExact(trimmed, "G", culture, TimeStyles, out value))
            {
                return true;
            }
        }

        return TimeSpan.TryParse(trimmed, culture, out value);
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? TimeSpan.MinValue;
            return true;
        }

        culture ??= CultureInfo.InvariantCulture;

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
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? TimeOnly.MinValue;
            return true;
        }

        culture ??= CultureInfo.InvariantCulture;

        if (timeFormats.Length > 0 &&
            TimeOnly.TryParseExact(trimmed, timeFormats, culture, DateTimeStyles, out value))
        {
            return true;
        }

        if (TimeOnly.TryParseExact(trimmed, "G", culture, DateTimeStyles, out value))
        {
            return true;
        }

        return TimeOnly.TryParse(trimmed, culture, out value);
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? TimeOnly.MinValue;
            return true;
        }

        culture ??= CultureInfo.InvariantCulture;

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
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? sbyte.MinValue;
        }
        else if (!sbyte.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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

    #region int (int32)
    public static int ParseInt(
        string input,
        int? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseInt(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseInt(
        string input,
        out int value,
        int? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? int.MinValue;
        }
        else if (!int.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
    }

    public static int? ParseNullableInt(
        string input,
        int? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableInt(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableInt(
        string input,
        out int? value,
        int? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? int.MinValue;
            return true;
        }

        if (int.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
    }
    #endregion

    #region uint
    public static uint ParseUInt(
        string input,
        uint? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseUInt(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseUInt(
        string input,
        out uint value,
        uint? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? uint.MinValue;
        }
        else if (!uint.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
    }

    public static uint? ParseNullableUInt(
        string input,
        uint? defaultValue,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        return TryOrThrow(TryParseNullableUInt(input, out var value, defaultValue, culture, trimmingOptions), input, value);
    }

    public static bool TryParseNullableUInt(
        string input,
        out uint? value,
        uint? defaultValue = null,
        IFormatProvider? culture = null,
        TrimmingOptions trimmingOptions = TrimmingOptions.Both)
    {
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? uint.MinValue;
            return true;
        }

        if (uint.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

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
        }
        else if (!long.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? long.MinValue;
            return true;
        }

        if (long.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
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
        }
        else if (!ulong.TryParse(trimmed, IntegerNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? ulong.MinValue;
            return true;
        }

        if (ulong.TryParse(trimmed, IntegerNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
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
        }
        else if (!float.TryParse(trimmed, FloatingPointNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? float.MinValue;
            return true;
        }

        if (float.TryParse(trimmed, FloatingPointNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
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
        }
        else if (!double.TryParse(trimmed, FloatingPointNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? double.MinValue;
            return true;
        }

        if (double.TryParse(trimmed, FloatingPointNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
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
        }
        else if (!decimal.TryParse(trimmed, FloatingPointNumberStyles, culture, out value))
        {
            return false;
        }

        return true;
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
        value = null;
        var trimmed = Trim(input, trimmingOptions);

        culture ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrEmpty(trimmed))
        {
            value = defaultValue ?? decimal.MinValue;
            return true;
        }

        if (decimal.TryParse(trimmed, FloatingPointNumberStyles, culture, out var parsedValue))
        {
            value = parsedValue;
            return false;
        }

        return false;
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