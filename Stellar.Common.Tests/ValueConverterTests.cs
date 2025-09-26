using System.Globalization;

namespace Stellar.Common.Tests;

public partial class ValueConverterTests
{
    #region helpers
    private static void ParseTestArguments<T>(
        T? defaultValue,
        string? codePage,
        out int index,
        out T? defValue,
        out CultureInfo culture)
    {
        index = (defaultValue is not null ? 1 : 0) | (codePage is not null ? 1 : 0) << 1;
        defValue = defaultValue ?? default;
        culture = ValueConverter.ParseCultureInfo(codePage);
    }

    private static int ParseTestArguments<T>(
        T? defaultValue,
        string? codePage,
        TrimmingOptions? trimmingOptions,
        out CultureInfo culture,
        out TrimmingOptions trimming)
    {
        culture = ValueConverter.ParseCultureInfo(codePage);
        trimming = trimmingOptions!.Value;

        return (defaultValue is not null ? 1 : 0) << 2 | (codePage is not null ? 1 : 0) << 1 | (trimmingOptions is not null ? 1 : 0);
    }
    #endregion

    #region formats
    [Theory]
    [MemberData(nameof(Formats))]
    public void SetsAndGetsDateTimeFormats(string format)
    {
        ValueConverter.AddDateTimeFormat(format);
        ValueConverter.AddTimeFormat(format);

        Assert.Contains(format, ValueConverter.GetDateTimeFormats());
        Assert.Contains(format, ValueConverter.GetTimeFormats());
    }
    #endregion

    #region to string
    [Theory]
    [MemberData(nameof(StringToString))]
    public void ToStringsString(string? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(CharToString))]
    public void ToStringsChar(char? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(ByteToString))]
    public void ToStringsByte(byte? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(SignedByteToString))]
    public void ToStringsSignedByte(sbyte? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(Shorts))]
    public void ToStringsShort(short? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(UInts))]
    public void ToStringsUInt(uint? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(UShorts))]
    public void ToStringsUShort(ushort? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(SingleToString))]
    public void ToStringsSingle(float? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(Doubles))]
    public void ToStringsDouble(double? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(IntToString))]
    public void ToStringsInt(int? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(ULongToString))]
    public void ToStringsULong(ulong? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(LongToString))]
    public void ToStringsLong(long? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(Decimals))]
    public void ToStringsDecimal(decimal? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(DateTimeOffsets))]
    public void ToStringsDateTimeOffset(DateTimeOffset? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(BooleanToString))]
    public void ToStringsBoolean(bool? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(GuidToString))]
    public void ToStringsGuid(string? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(new Guid(obj!), format));
    }

    [Theory]
    [MemberData(nameof(BucketToString))]
    public void ToStringsBucket(string[] obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(new Bucket<string>(obj!), format));
    }

    [Theory]
    [MemberData(nameof(DateToString))]
    public void ToStringsDate(DateOnly? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(DateTimeToString))]
    public void ToStringsDateTime(DateTime? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(TimeToString))]
    public void ToStringsTime(TimeOnly? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    [Theory]
    [MemberData(nameof(TimeSpanToString))]
    public void ToStringsTimeSpan(TimeSpan? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }
    #endregion

    #region sbyte
    [Theory]
    [MemberData(nameof(SignedByteData))]
    public void ParsesSByteOrThrows(string input, sbyte? defaultValue, string? codePage, bool expectedResult, sbyte? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<sbyte> action = index switch
        {
            3 => () => ValueConverter.ParseSByte(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseSByte(input, culture: culture),
            1 => () => ValueConverter.ParseSByte(input, defValue!.Value),
            _ => () => ValueConverter.ParseSByte(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(SignedByteData))]
    public void ParsesNullableSByteOrThrows(string input, sbyte? _, string? codePage, bool expectedResult, sbyte? expected)
    {
        ParseTestArguments((sbyte?)null, codePage, out var index, out var _, out var culture);

        Func<sbyte?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableSByte(input, culture),
            _ => () => ValueConverter.ParseNullableSByte(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region byte
    [Theory]
    [MemberData(nameof(ByteData))]
    public void ParsesByteOrThrows(string input, byte? defaultValue, string? codePage, bool expectedResult, byte? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<byte> action = index switch
        {
            3 => () => ValueConverter.ParseByte(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseByte(input, culture: culture),
            1 => () => ValueConverter.ParseByte(input, defValue!.Value),
            _ => () => ValueConverter.ParseByte(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(ByteData))]
    public void ParsesNullableByteOrThrows(string input, byte? _, string? codePage, bool expectedResult, byte? expected)
    {
        ParseTestArguments((byte?)null, codePage, out var index, out var _, out var culture);

        Func<byte?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableByte(input, culture),
            _ => () => ValueConverter.ParseNullableByte(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region short
    [Theory]
    [MemberData(nameof(ShortData))]
    public void ParsesShortOrThrows(string input, short? defaultValue, string? codePage, bool expectedResult, short? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<short> action = index switch
        {
            3 => () => ValueConverter.ParseShort(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseShort(input, culture: culture),
            1 => () => ValueConverter.ParseShort(input, defValue!.Value),
            _ => () => ValueConverter.ParseShort(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(ShortData))]
    public void ParsesNullableShortOrThrows(string input, short? _, string? codePage, bool expectedResult, short? expected)
    {
        ParseTestArguments((short?)null, codePage, out var index, out var _, out var culture);

        Func<short?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableShort(input, culture),
            _ => () => ValueConverter.ParseNullableShort(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region ushort
    [Theory]
    [MemberData(nameof(UnsignedShortData))]
    public void ParsesUnsignedShortOrThrows(string input, ushort? defaultValue, string? codePage, bool expectedResult, ushort? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<ushort> action = index switch
        {
            3 => () => ValueConverter.ParseUnsignedShort(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseUnsignedShort(input, culture: culture),
            1 => () => ValueConverter.ParseUnsignedShort(input, defValue!.Value),
            _ => () => ValueConverter.ParseUnsignedShort(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(UnsignedShortData))]
    public void ParsesNullableUnsignedShortOrThrows(string input, ushort? _, string? codePage, bool expectedResult, ushort? expected)
    {
        ParseTestArguments((ushort?)null, codePage, out var index, out var _, out var culture);

        Func<ushort?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableUnsignedShort(input, culture),
            _ => () => ValueConverter.ParseNullableUnsignedShort(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region int
    [Theory]
    [MemberData(nameof(IntegerData))]
    public void ParsesIntOrThrows(string input, int? defaultValue, string? codePage, bool expectedResult, int expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<int> action = index switch
        {
            3 => () => ValueConverter.ParseInt(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseInt(input, culture: culture),
            1 => () => ValueConverter.ParseInt(input, defValue),
            _ => () => ValueConverter.ParseInt(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(IntegerData))]
    public void ParsesNullableIntOrThrows(string input, int? _, string? codePage, bool expectedResult, int expected)
    {
        ParseTestArguments((int?)null, codePage, out var index, out var _, out var culture);

        Func<int?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableInt(input, culture),
            _ => () => ValueConverter.ParseNullableInt(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region uint
    [Theory]
    [MemberData(nameof(UnsignedIntegerData))]
    public void ParsesUIntOrThrows(string input, uint? defaultValue, string? codePage, bool expectedResult, uint expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<uint> action = index switch
        {
            3 => () => ValueConverter.ParseUInt(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseUInt(input, culture: culture),
            1 => () => ValueConverter.ParseUInt(input, defValue),
            _ => () => ValueConverter.ParseUInt(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(UnsignedIntegerData))]
    public void ParsesNullableUIntOrThrows(string input, uint? _, string? codePage, bool expectedResult, uint expected)
    {
        ParseTestArguments((uint?)null, codePage, out var index, out var _, out var culture);

        Func<uint?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableUInt(input, culture),
            _ => () => ValueConverter.ParseNullableUInt(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region long
    [Theory]
    [MemberData(nameof(LongIntegerData))]
    public void ParsesLongOrThrows(string input, long? defaultValue, string? codePage, bool expectedResult, long expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<long> action = index switch
        {
            3 => () => ValueConverter.ParseLong(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseLong(input, culture: culture),
            1 => () => ValueConverter.ParseLong(input, defValue),
            _ => () => ValueConverter.ParseLong(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(LongIntegerData))]
    public void ParsesNullableLongOrThrows(string input, long? _, string? codePage, bool expectedResult, long expected)
    {
        ParseTestArguments((long?)null, codePage, out var index, out var _, out var culture);

        Func<long?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableLong(input, culture),
            _ => () => ValueConverter.ParseNullableLong(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region ulong
    [Theory]
    [MemberData(nameof(UnsignedLongIntegerData))]
    public void ParsesUnsignedLongOrThrows(string input, ulong? defaultValue, string? codePage, bool expectedResult, ulong expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<ulong> action = index switch
        {
            3 => () => ValueConverter.ParseULong(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseULong(input, culture: culture),
            1 => () => ValueConverter.ParseULong(input, defValue),
            _ => () => ValueConverter.ParseULong(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(UnsignedLongIntegerData))]
    public void ParsesNullableUnsignedLongOrThrows(string input, ulong? _, string? codePage, bool expectedResult, ulong expected)
    {
        ParseTestArguments((ulong?)null, codePage, out var index, out var _, out var culture);

        Func<ulong?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableULong(input, culture),
            _ => () => ValueConverter.ParseNullableULong(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region float
    [Theory]
    [MemberData(nameof(FloatData))]
    public void ParsesFloatOrThrows(string input, float? defaultValue, string? codePage, bool expectedResult, float expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<float> action = index switch
        {
            3 => () => ValueConverter.ParseFloat(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseFloat(input, culture: culture),
            1 => () => ValueConverter.ParseFloat(input, defValue),
            _ => () => ValueConverter.ParseFloat(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(FloatData))]
    public void ParsesNullableFloatOrThrows(string input, float? _, string? codePage, bool expectedResult, float expected)
    {
        ParseTestArguments((float?)null, codePage, out var index, out var _, out var culture);

        Func<float?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableFloat(input, culture),
            _ => () => ValueConverter.ParseNullableFloat(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region double
    [Theory]
    [MemberData(nameof(DoubleData))]
    public void ParsesDoubleOrThrows(string input, double? defaultValue, string? codePage, bool expectedResult, double expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<double> action = index switch
        {
            3 => () => ValueConverter.ParseDouble(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseDouble(input, culture: culture),
            1 => () => ValueConverter.ParseDouble(input, defValue),
            _ => () => ValueConverter.ParseDouble(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(DoubleData))]
    public void ParsesNullableDoubleOrThrows(string input, double? _, string? codePage, bool expectedResult, double expected)
    {
        ParseTestArguments((double?)null, codePage, out var index, out var _, out var culture);

        Func<double?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableDouble(input, culture),
            _ => () => ValueConverter.ParseNullableDouble(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region decimal
    [Theory]
    [MemberData(nameof(DecimalData))]
    public void ParsesDecimalOrThrows(string input, decimal? defaultValue, string? codePage, bool expectedResult, decimal expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<decimal> action = index switch
        {
            3 => () => ValueConverter.ParseDecimal(input, defValue, culture: culture),
            2 => () => ValueConverter.ParseDecimal(input, culture: culture),
            1 => () => ValueConverter.ParseDecimal(input, defValue),
            _ => () => ValueConverter.ParseDecimal(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(DecimalData))]
    public void ParsesNullableDecimalOrThrows(string input, decimal? _, string? codePage, bool expectedResult, decimal expected)
    {
        ParseTestArguments((decimal?)null, codePage, out var index, out var _, out var culture);

        Func<decimal?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableDecimal(input, culture),
            _ => () => ValueConverter.ParseNullableDecimal(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region bool
    [Theory]
    [MemberData(nameof(BooleanData))]
    public void ParsesBooleanOrThrows(string input, bool? defaultValue, bool expectedResult, bool expected)
    {
        ParseTestArguments(defaultValue, null, out var index, out var defValue, out var culture);

        bool action() => ValueConverter.ParseBoolean(input);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(BooleanData))]
    public void ParsesNullableBooleanOrThrows(string input, bool? _, bool expectedResult, bool expected)
    {
        bool? action() => ValueConverter.ParseNullableBoolean(input);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region char
    [Theory]
    [MemberData(nameof(CharData))]
    public void ParsesCharOrThrows(string input, char? defaultValue, TrimmingOptions trimmingOptions, bool expectedResult, char expected)
    {
        var index = defaultValue is not null
            ? 1
            : 0;

        char action() => ValueConverter.ParseChar(input, trimmingOptions: trimmingOptions);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(CharData))]
    public void ParsesNullableCharOrThrows(string input, char? _, TrimmingOptions trimmingOptions, bool expectedResult, char expected)
    {
        char? action() => ValueConverter.ParseNullableChar(input, trimmingOptions);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region enum
    [Theory]
    [MemberData(nameof(EnumData))]
    public void GenericParsesEnumOrThrows(string input, Enum1? defaultValue, TrimmingOptions? trimmingOptions, bool expectedResult, Enum1? expected)
    {
        var index = (defaultValue is not null ? 1 : 0) | (trimmingOptions is not null ? 1 : 0) << 1;

        Func<Enum1> action = index switch
        {
            3 => () => ValueConverter.ParseEnum(input, defaultValue!.Value, trimmingOptions!.Value),
            2 => () => ValueConverter.ParseEnum<Enum1>(input, trimmingOptions: trimmingOptions!.Value),
            1 => () => ValueConverter.ParseEnum(input, defaultValue!.Value),
            _ => () => ValueConverter.ParseEnum<Enum1>(input)
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(EnumData))]
    public void ParsesEnumOrThrows(string input, Enum1? defaultValue, TrimmingOptions? trimmingOptions, bool expectedResult, Enum1? expected)
    {
        var index = (defaultValue is not null ? 1 : 0) | (trimmingOptions is not null ? 1 : 0) << 1;

        Func<object> action = index switch
        {
            3 => () => ValueConverter.ParseEnum(typeof(Enum1), input, defaultValue!.Value, trimmingOptions!.Value),
            _ => () => ValueConverter.ParseEnum<Enum1>(input)
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(EnumData))]
    public void ParsesNullableEnumOrThrows(string input, Enum1? _, TrimmingOptions? trimmingOptions, bool expectedResult, Enum1? expected)
    {
        Func<Enum1?> action = (trimmingOptions is not null ? 1 : 0) switch
        {
            1 => () => ValueConverter.ParseNullableEnum<Enum1>(input, trimmingOptions!.Value),
            _ => () => ValueConverter.ParseNullableEnum<Enum1>(input)
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region Guid
    [Theory]
    [MemberData(nameof(GuidData))]
    public void ParsesGuidOrThrows(string input, Guid? defaultValue, bool expectedResult, Guid? expected)
    {
        var index = (defaultValue is not null ? 1 : 0);
        var defValue = defaultValue ?? default;

        Guid action() => ValueConverter.ParseGuid(input);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(GuidData))]
    public void ParsesNullableGuidOrThrows(string input, Guid? _, bool expectedResult, Guid? expected)
    {
        Guid? action() => ValueConverter.ParseNullableGuid(input);

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region date
    [Theory]
    [MemberData(nameof(DateData))]
    public void ParsesDateOnlyOrThrows(string input, DateOnly? defaultValue, string? codePage, bool expectedResult, DateOnly? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<DateOnly> action = index switch
        {
            3 => () => ValueConverter.ParseDate(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseDate(input, culture: culture),
            1 => () => ValueConverter.ParseDate(input, defValue!.Value),
            _ => () => ValueConverter.ParseDate(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(DateData))]
    public void ParsesNullableDateOnlyOrThrows(string input, DateOnly? _, string? codePage, bool expectedResult, DateOnly? expected)
    {
        ParseTestArguments((DateOnly?)null, codePage, out var index, out var _, out var culture);

        Func<DateOnly?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableDate(input, culture),
            _ => () => ValueConverter.ParseNullableDate(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region time
    [Theory]
    [MemberData(nameof(TimeData))]
    public void ParsesTimeOrThrows(string input, TimeOnly? defaultValue, string? codePage, bool expectedResult, TimeOnly? expected)
    {
        ValueConverter.AddTimeFormat("HH.mm.ss");

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<TimeOnly> action = index switch
        {
            3 => () => ValueConverter.ParseTime(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseTime(input, culture: culture),
            1 => () => ValueConverter.ParseTime(input, defValue!.Value),
            _ => () => ValueConverter.ParseTime(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(TimeData))]
    public void ParsesNullableTimeOrThrows(string input, TimeOnly? _, string? codePage, bool expectedResult, TimeOnly? expected)
    {
        ValueConverter.AddTimeFormat("HH.mm.ss");

        ParseTestArguments((TimeOnly?)null, codePage, out var index, out var _, out var culture);

        Func<TimeOnly?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableTime(input, culture),
            _ => () => ValueConverter.ParseNullableTime(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region datetime
    [Theory]
    [MemberData(nameof(DateTimeData))]
    public void ParsesDateTimeOrThrows(string input, DateTime? defaultValue, string? codePage, bool expectedResult, DateTime? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<DateTime> action = index switch
        {
            3 => () => ValueConverter.ParseDateTime(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseDateTime(input, culture: culture),
            1 => () => ValueConverter.ParseDateTime(input, defValue!.Value),
            _ => () => ValueConverter.ParseDateTime(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeData))]
    public void ParsesNullableDateTimeOrThrows(string input, DateTime? _, string? codePage, bool expectedResult, DateTime? expected)
    {
        ParseTestArguments((DateTime?)null, codePage, out var index, out var _, out var culture);

        Func<DateTime?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableDateTime(input, culture),
            _ => () => ValueConverter.ParseNullableDateTime(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region datetime offset
    [Theory]
    [MemberData(nameof(DateTimeOffsetData))]
    public void ParsesDateTimeOffsetOrThrows(string input, DateTimeOffset? defaultValue, string? codePage, bool expectedResult, DateTimeOffset? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<DateTimeOffset> action = index switch
        {
            3 => () => ValueConverter.ParseDateTimeOffset(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseDateTimeOffset(input, culture: culture),
            1 => () => ValueConverter.ParseDateTimeOffset(input, defValue!.Value),
            _ => () => ValueConverter.ParseDateTimeOffset(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeOffsetData))]
    public void ParsesNullableDateTimeOffsetOrThrows(string input, DateTimeOffset? _, string? codePage, bool expectedResult, DateTimeOffset? expected)
    {
        ParseTestArguments((DateTimeOffset?)null, codePage, out var index, out var _, out var culture);

        Func<DateTimeOffset?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableDateTimeOffset(input, culture),
            _ => () => ValueConverter.ParseNullableDateTimeOffset(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region timespan
    [Theory]
    [MemberData(nameof(TimeSpanData))]
    public void ParsesTimeSpanOrThrows(string input, TimeSpan? defaultValue, string? codePage, bool expectedResult, TimeSpan? expected)
    {
        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<TimeSpan> action = index switch
        {
            3 => () => ValueConverter.ParseTimeSpan(input, defValue!.Value, culture: culture),
            2 => () => ValueConverter.ParseTimeSpan(input, culture: culture),
            1 => () => ValueConverter.ParseTimeSpan(input, defValue!.Value),
            _ => () => ValueConverter.ParseTimeSpan(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }

    [Theory]
    [MemberData(nameof(TimeSpanData))]
    public void ParsesNullableTimeSpanOrThrows(string input, TimeSpan? _, string? codePage, bool expectedResult, TimeSpan? expected)
    {
        ParseTestArguments((TimeSpan?)null, codePage, out var index, out var _, out var culture);

        Func<TimeSpan?> action = index switch
        {
            2 => () => ValueConverter.ParseNullableTimeSpan(input, culture),
            _ => () => ValueConverter.ParseNullableTimeSpan(input),
        };

        if (!expectedResult)
        {
            Assert.Throws<FormatException>(() => action());
        }
        else
        {
            Assert.Equal(expected, action());
        }
    }
    #endregion

    #region parse
    [Theory]
    [MemberData(nameof(ParseTypeData))]
    public void ParsesOrThrows(string input, Type type, object? defaultValue, string codePage, TrimmingOptions? trimmingOptions, bool result, object? expectedValue)
    {
        var index = ParseTestArguments(defaultValue, codePage, trimmingOptions, out var culture, out var trimming);

        Func<object> action = index switch
        {
            7 => () => ValueConverter.Parse(input, type, defaultValue, culture, trimming),
            6 => () => ValueConverter.Parse(input, type, defaultValue, culture),
            5 => () => ValueConverter.Parse(input, type, defaultValue, trimmingOptions: trimming),
            4 => () => ValueConverter.Parse(input, type, defaultValue),
            3 => () => ValueConverter.Parse(input, type, culture: culture, trimmingOptions: trimming),
            2 => () => ValueConverter.Parse(input, type, culture: culture),
            1 => () => ValueConverter.Parse(input, type, trimmingOptions: trimming),
            _ => () => ValueConverter.Parse(input, type)
        };

        if (!result)
        {
            if (type == typeof(string) || type.IsValueType)
            {
                Assert.Throws<FormatException>(() => action());
            }
            else
            {
                Assert.Throws<NotSupportedException>(() => action());
            }
        }
        else
        {
            Assert.Equal(expectedValue, action());
        }
    }

    [Fact]
    public void ParsesGeneric()
    {
        Assert.Equal(123, ValueConverter.Parse<int>("123"));
        
        Assert.True(ValueConverter.TryParse<double>("42.67", out var _));
        
        Assert.True(ValueConverter.TryParse("20250916", out var _, new DateOnly(1, 1, 1)));
        Assert.False(ValueConverter.TryParse("20251609", out var d, new DateOnly(1, 1, 1)));
        Assert.Equal(new DateOnly(1, 1, 1), d);
        
        Assert.False(ValueConverter.TryParse<sbyte>("257", out var b, -1));
        Assert.Equal(-1, b);
    }
    #endregion
}
