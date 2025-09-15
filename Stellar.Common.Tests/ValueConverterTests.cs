using System;
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
    #endregion

    #region formats
    public static TheoryData<string> Formats =>
    [
        "yyyy-MM-dd",
        "HH.mm.ss"
    ];

    [Theory]
    [MemberData(nameof(Formats))]
    public void SetsAndGetsDateTimeFormats(string format)
    {
        ValueConverter.AddDateTimeFormat(format);
        ValueConverter.AddDateTimeFormats([format]);

        var formats = ValueConverter.GetDateTimeFormats();

        Assert.Contains(format, formats);
    }
    #endregion

    #region to string
    public static TheoryData<string?, string, string?> StringToString => new()
    {
        { "", "", null },
        { null, "", null },
        { "dim", "       dim", "0,10" }
    };

    [Theory]
    [MemberData(nameof(StringToString))]
    public void ToStringsString(string? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<char?, string, string?> CharToString => new()
    {
        { ' ', " ", null },
        { '\u2192', "\u2192", null },
        { 'A', "A", null }
    };

    [Theory]
    [MemberData(nameof(CharToString))]
    public void ToStringsChar(char? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<byte?, string, string?> ByteToString => new()
    {
        { 128, "80", "X2" },
        { 255, "FF", "X2" },
        { 192, "0192", "D4" }
    };

    [Theory]
    [MemberData(nameof(ByteToString))]
    public void ToStringsByte(byte? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<sbyte?, string, string?> SignedByteToString => new()
    {
        { -80, "B0", "X2" },
        { 127, "7F", "X2" },
    };

    [Theory]
    [MemberData(nameof(SignedByteToString))]
    public void ToStringsSignedByte(sbyte? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<short?, string, string?> Shorts => new()
    {
        { 12356, "12,356", "N0" }
    };

    [Theory]
    [MemberData(nameof(Shorts))]
    public void ToStringsShort(short? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<uint?, string, string?> UInts => new()
    {
        { 256 , "100" , "X2" }
    };

    [Theory]
    [MemberData(nameof(UInts))]
    public void ToStringsUInt(uint? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<ushort?, string, string?> UShorts => new()
    {
        { 80 , "50" , "X2" }
    };

    [Theory]
    [MemberData(nameof(UShorts))]
    public void ToStringsUShort(ushort? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<float?, string, string?> SingleToString => new()
    {
        { 1.54321E-3f, "1.54321E-003", "E5" }
    };

    [Theory]
    [MemberData(nameof(SingleToString))]
    public void ToStringsSingle(float? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<double?, string, string?, string?> Doubles => new()
    {
        { 1.5E-5d, "0.0000150000", "N10", null },
        { 1.5E-5d, "1.5E-05", null, null },
        { 123.456, "123,456", null, "de-DE" },
        { 123.456, "123.456", null, null }
    };

    [Theory]
    [MemberData(nameof(Doubles))]
    public void ToStringsDouble(double? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<int?, string, string?, string?> IntToString => new()
    {
        { 123456, "123,456", "N0", null },
        { 123456, "123.456", "N0", "de-DE" }
    };

    [Theory]
    [MemberData(nameof(IntToString))]
    public void ToStringsInt(int? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<ulong?, string, string?, string?> ULongToString => new()
    {
        { 1234567890121UL, "1,234,567,890,121", "N0", null },
        { 1234567890121UL, "1.234.567.890.121", "N0" , "de-DE"},
        { 75849584532321U, "75,849,584,532,321", "N0", null },
        { 75849584532321U, "75 849 584 532 321", "### ### ### ### ###", null },
    };

    [Theory]
    [MemberData(nameof(ULongToString))]
    public void ToStringsULong(ulong? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<long?, string, string?, string?> LongToString => new()
    {
        { -123456L, "-123,456", "N0", null },
        { 123456L, "123.456", "N0", "de-DE" }
    };

    [Theory]
    [MemberData(nameof(LongToString))]
    public void ToStringsLong(long? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal?, string, string?, string?> Decimals => new()
    {
        { 1987.6m, "$1,987.60", "C2", "en-US" }
    };

    [Theory]
    [MemberData(nameof(Decimals))]
    public void ToStringsDecimal(decimal? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<DateTimeOffset?, string, string?, string?> DateTimeOffsets => new()
    {
        { DateTimeOffset.FromUnixTimeSeconds(7), "00:00:07", "HH:mm:ss", null }
    };

    [Theory]
    [MemberData(nameof(DateTimeOffsets))]
    public void ToStringsDateTimeOffset(DateTimeOffset? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<bool?, string, string?> BooleanToString => new()
    {
        { false, "False", "'Y','N'" },
        { false, "False", null },
        { true,  "True", "'Y','N'"},
        { true, "True", null }
    };

    [Theory]
    [MemberData(nameof(BooleanToString))]
    public void ToStringsBoolean(bool? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<string?, string, string?> GuidToString => new()
    {
        { $"{Guid.Empty}", "(00000000-0000-0000-0000-000000000000)", "P" },
        { $"{new Guid("e9cc294d-0a31-481b-bc61-f677c1392516")}", "{0xe9cc294d,0x0a31,0x481b,{0xbc,0x61,0xf6,0x77,0xc1,0x39,0x25,0x16}}", "X" },
    };

    [Theory]
    [MemberData(nameof(GuidToString))]
    public void ToStringsGuid(string? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(new Guid(obj!), format));
    }

    public static TheoryData<string[], string, string?> BucketToString => new()
    {
        { ["a", "b"], "a:0 b:0", "JSON" },
        { ["a", "b"], "a:0 b:0", null }
    };

    [Theory]
    [MemberData(nameof(BucketToString))]
    public void ToStringsBucket(string[] obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(new Bucket<string>(obj!), format));
    }

    public static TheoryData<DateOnly?, string, string?> DateToString => new()
    {
        { new DateOnly(2025, 7, 18), "07/18/2025", null },
        { new DateOnly(2025, 7, 18), "2025-Jul-18", "yyyy-MMM-dd" },
    };

    [Theory]
    [MemberData(nameof(DateToString))]
    public void ToStringsDate(DateOnly? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<DateTime?, string, string?, string?> DateTimeToString => new()
    {
        { new DateTime(2025, 12, 31), "2025-Dec-31 00:00:00", "yyyy-MMM-dd HH:mm:ss", "en-US" },
        { new DateTime(2025, 7, 18), "07/18/2025 00:00:00", null, null },
        { new DateTime(2025, 7, 18), "7/18/2025 12:00:00 AM", null, "en-US" },
        { new DateTime(2025, 7, 18, 20, 45, 32), "2025-Jul-18 20:45:32", "yyyy-MMM-dd HH:mm:ss", null }
    };

    [Theory]
    [MemberData(nameof(DateTimeToString))]
    public void ToStringsDateTime(DateTime? obj, string expected, string? format, string? codePage)
    {
        var result = codePage is not null
            ? ValueConverter.ToString(obj!, format, codePage: codePage)
            : ValueConverter.ToString(obj!, format);

        Assert.Equal(expected, result);
    }

    public static TheoryData<TimeOnly?, string, string?> TimeToString => new()
    {
        { new TimeOnly(20, 45, 32), "08.45.32", "hh.mm.ss" },
        { new TimeOnly(20, 45, 32), "20:45", null },
        { null!, "", null }
    };

    [Theory]
    [MemberData(nameof(TimeToString))]
    public void ToStringsTime(TimeOnly? obj, string expected, string? format)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format));
    }

    public static TheoryData<TimeSpan?, string, string?, string?> TimeSpanToString => new()
    {
        { new TimeSpan(8, 45, 32), "08:45:32", null, "fi-FI" },
        { new TimeSpan(8, 45, 32), "08:45:32", null, null }
    };

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

    #region int
    [Theory]
    [MemberData(nameof(IntegerData))]
    public void TriesToParseInt(string input, int? defaultValue, string? codePage, bool expectedResult, int expected)
    {
        int value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseInt(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseInt(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseInt(input, out value, defValue),
            _ => () => ValueConverter.TryParseInt(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableInt(string input, int? _, string? codePage, bool expectedResult, int expected)
    {
        int? value = null;

        ParseTestArguments((int?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableInt(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableInt(input, out value),
        };

        Assert.Equal(expectedResult, action());

        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseUInt(string input, uint? defaultValue, string? codePage, bool expectedResult, uint expected)
    {
        uint value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseUInt(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseUInt(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseUInt(input, out value, defValue),
            _ => () => ValueConverter.TryParseUInt(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableUInt(string input, uint? _, string? codePage, bool expectedResult, uint expected)
    {
        uint? value = null;

        ParseTestArguments((uint?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableUInt(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableUInt(input, out value),
        };

        Assert.Equal(expectedResult, action());

        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseLong(string input, long? defaultValue, string? codePage, bool expectedResult, long expected)
    {
        long value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseLong(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseLong(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseLong(input, out value, defValue),
            _ => () => ValueConverter.TryParseLong(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableLong(string input, long? _, string? codePage, bool expectedResult, long expected)
    {
        long? value = null;

        ParseTestArguments((long?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableLong(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableLong(input, out value),
        };

        Assert.Equal(expectedResult, action());

        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseULong(string input, ulong? defaultValue, string? codePage, bool expectedResult, ulong expected)
    {
        ulong value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseULong(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseULong(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseULong(input, out value, defValue),
            _ => () => ValueConverter.TryParseULong(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableUnsignedLong(string input, ulong? _, string? codePage, bool expectedResult, ulong expected)
    {
        ulong? value = null;

        ParseTestArguments((ulong?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableULong(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableULong(input, out value),
        };

        Assert.Equal(expectedResult, action());
        
        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseFloat(string input, float? defaultValue, string? codePage, bool result, float expected)
    {
        float value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseFloat(input, out value, defValue, culture),
            2 => () => ValueConverter.TryParseFloat(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseFloat(input, out value, defValue),
            _ => () => ValueConverter.TryParseFloat(input, out value),
        };

        Assert.Equal(result, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableFloat(string input, float? _, string? codePage, bool expectedResult, float expected)
    {
        float? value = null;

        ParseTestArguments((float?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableFloat(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableFloat(input, out value),
        };

        Assert.Equal(expectedResult, action());
        
        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseDouble(string input, double? defaultValue, string? codePage, bool expectedResult, double expected)
    {
        double value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseDouble(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseDouble(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseDouble(input, out value, defValue),
            _ => () => ValueConverter.TryParseDouble(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableDouble(string input, double? _, string? codePage, bool expectedResult, double expected)
    {
        double? value = null;

        ParseTestArguments((double?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableDouble(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableDouble(input, out value),
        };

        Assert.Equal(expectedResult, action());
        
        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseDecimal(string input, decimal? defaultValue, string? codePage, bool expectedResult, decimal expected)
    {
        decimal value = default;

        ParseTestArguments(defaultValue, codePage, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            3 => () => ValueConverter.TryParseDecimal(input, out value, defValue, culture: culture),
            2 => () => ValueConverter.TryParseDecimal(input, out value, culture: culture),
            1 => () => ValueConverter.TryParseDecimal(input, out value, defValue),
            _ => () => ValueConverter.TryParseDecimal(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

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
    public void TriesToParseNullableDecimal(string input, decimal? _, string? codePage, bool expectedResult, decimal expected)
    {
        decimal? value = null;

        ParseTestArguments((decimal?)null, codePage, out var index, out var _, out var culture);

        Func<bool> action = index switch
        {
            2 => () => ValueConverter.TryParseNullableDecimal(input, out value, culture),
            _ => () => ValueConverter.TryParseNullableDecimal(input, out value),
        };

        Assert.Equal(expectedResult, action());
        
        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseBoolean(string input, bool? defaultValue, bool expectedResult, bool expected)
    {
        bool value = default;

        ParseTestArguments(defaultValue, null, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            1 => () => ValueConverter.TryParseBoolean(input, out value, defValue),
            _ => () => ValueConverter.TryParseBoolean(input, out value),
        };

        Assert.Equal(expectedResult, action());
        Assert.Equal(expected, value);
    }

    [Theory]
    [MemberData(nameof(BooleanData))]
    public void ParsesBooleanOrThrows(string input, bool? defaultValue, bool expectedResult, bool expected)
    {
        ParseTestArguments(defaultValue, null, out var index, out var defValue, out var culture);

        Func<bool> action = index switch
        {
            1 => () => ValueConverter.ParseBoolean(input, defValue),
            _ => () => ValueConverter.ParseBoolean(input),
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
    [MemberData(nameof(BooleanData))]
    public void TriesToParseNullableBoolean(string input, bool? _, bool expectedResult, bool expected)
    {

        Assert.Equal(expectedResult, ValueConverter.TryParseNullableBoolean(input, out bool? value));

        if (expectedResult)
        {
            Assert.Equal(expected, value);
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
    public void TriesToParseChar(string input, char? defaultValue, bool expectedResult, char expected)
    {
        var result = defaultValue is not null
            ? ValueConverter.TryParseChar(input, out char value, defaultValue.Value)
            : ValueConverter.TryParseChar(input, out value);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expected, value);
    }

    [Theory]
    [MemberData(nameof(CharData))]
    public void ParsesCharOrThrows(string input, char? defaultValue, bool expectedResult, char expected)
    {
        var index = defaultValue is not null
            ? 1
            : 0;

        Func<char> action = index switch
        {
            1 => () => ValueConverter.ParseChar(input, defaultValue!.Value),
            _ => () => ValueConverter.ParseChar(input),
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
    [MemberData(nameof(CharData))]
    public void TriesToParseNullableChar(string input, char? defaultValue, bool expectedResult, char expected)
    {
        char? value = null;

        var result = defaultValue is not null
            ? ValueConverter.TryParseNullableChar(input, out value)
            : ValueConverter.TryParseNullableChar(input, out value);

        Assert.Equal(expectedResult, result);
        
        if (!expectedResult)
        {
            Assert.Null(value);
        }
        else
        {
            Assert.Equal(expected, value);
        }
    }

    [Theory]
    [MemberData(nameof(CharData))]
    public void ParsesNullableCharOrThrows(string input, char? _, bool expectedResult, char expected)
    {
        char? action() => ValueConverter.ParseNullableChar(input);

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

}
