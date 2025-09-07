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

    #region boolean
    public static TheoryData<bool?, bool?, bool?> Booleans => new()
    {
        { true, null, true },
        { false, null, false },
        { null, true, true }
    };

    [Theory]
    [MemberData(nameof(Booleans))]
    public void ParsesBoolean(bool? input, object? defaultValue, bool? expected)
    {
        Assert.Equal(expected, ValueConverter.Parse(input, typeof(bool), defaultValue));
    }

    public static TheoryData<string, bool> TruthyAndFalsy => new()
    {
        { "True", true },
        { " true", true },
        { "TRUE ", true },
        { "False", false },
        { " false", false },
        { "FALSE ", false }
    };

    [Theory]
    [MemberData(nameof(TruthyAndFalsy))]
    public void ParsesBooleanString(string input, bool expected)
    {
        Assert.Equal(expected, ValueConverter.ParseBoolean(input));
    }

    public static TheoryData<string, bool?> NullableTruthyAndFalsy => new()
    {
        { "True", true },
        { "FALSE ", false }
    };

    [Theory]
    [MemberData(nameof(NullableTruthyAndFalsy))]
    public void ToNullableBoolean(string input, bool? expected)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableBoolean(input));
    }

    public static TheoryData<string> NotBoolean => new()
    {
        { "YES" },
        { "NO" },
        { "ON" },
        { "OFF" },
        { "T" },
        { "F" },
        { "2" },
        { "-1" },
        { "0" },
        { " 0 " },
        { "1" },
        { " 1 " },
        { "" },
        { null! }
    };
    [Theory]
    [MemberData(nameof(NotBoolean))]
    public void ToBooleanThrows(string input)
    {
        Assert.Throws<FormatException>(() => ValueConverter.ParseBoolean(input));
    }
    #endregion

    #region char
    public static TheoryData<string, char?, TrimmingOptions> Chars => new()
    {
        { "\u2192 ", '\u2192', TrimmingOptions.Both },
        { " T", 'T', TrimmingOptions.Start },
        { "Y   ", 'Y', TrimmingOptions.End },
        { " F ", 'F' , TrimmingOptions.Both},
        { " ", ' ', TrimmingOptions.None },
        { "   ↑   ", '\u2191', TrimmingOptions.Both },
        { "   \u2191   ", '↑', TrimmingOptions.Both }
    };

    [Theory]
    [MemberData(nameof(Chars))]
    public void ToNullableChar(string input, char? expected, TrimmingOptions trimmingOptions)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableChar(input!, trimmingOptions));
    }
    #endregion

    #region enum
    public static TheoryData<string, TrimmingOptions> Enum1 => new()
    {
        { "   Both   ", TrimmingOptions.Both },
        { "Start ", TrimmingOptions.Start },
        { "   End", TrimmingOptions.End },
        { "None", TrimmingOptions.None }
    };

    [Theory]
    [MemberData(nameof(Enum1))]
    public void ToEnum(string input, TrimmingOptions expected)
    {
        Assert.Equal(expected, ValueConverter.ParseEnum<TrimmingOptions>(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Extra")]
    public void ParseEnumThrows(string input)
    {
        Assert.Throws<FormatException>(() => ValueConverter.ParseEnum<TrimmingOptions>(input));
    }


    public static TheoryData<string, TrimmingOptions?> Enum2 => new()
    {
        { "   Both   ", TrimmingOptions.Both },
        { "Start ", TrimmingOptions.Start },
        { "   End", TrimmingOptions.End },
        { "None", TrimmingOptions.None },
        { "", null }
    };

    [Theory]
    [MemberData(nameof(Enum2))]
    public void ParseNullableEnum(string input, TrimmingOptions? expected)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableEnum<TrimmingOptions>(input));
    }
    #endregion

    #region datetime
    public static TheoryData<string, DateTime> DateTimes => new()
    {
        { "2024-08-09 03:25:05.2456256", new DateTime(2024, 8, 9, 3, 25, 5, 245, 625).AddMicroseconds(.6) },
        { "2023-12-02 02:39:14.345", new DateTime(2023, 12, 2, 2, 39, 14, 345) },
        { "2024-06-06 07:47:55.5676", new DateTime(2024, 6, 6, 7, 47, 55, 567, 600) },
        { "2023-11-29 11:45:55.5464", new DateTime(2023, 11, 29, 11, 45, 55, 546, 400) },
        { "2024-04-01 03:58:25.134", new DateTime(2024, 4, 1, 3, 58, 25, 134) },
        { "2024-06-26 09:41:24.00001", new DateTime(2024, 6, 26, 9, 41, 24, 0, 10) },
        { "2023-10-20 02:40:13.0001", new DateTime(2023, 10, 20, 2, 40, 13, 0, 100) },
        { "2024-06-29 13:21:25.999", new DateTime(2024, 6, 29, 13, 21, 25, 999) },
        { "2023-09-18 17:18:55.88", new DateTime(2023, 9, 18, 17, 18, 55, 880) },
        { "2023-11-16 02:33:52.7", new DateTime(2023, 11, 16, 2, 33, 52, 700) },
        { "2024-05-03 17:55:04.65", new DateTime(2024, 5, 3, 17, 55, 4, 650) },
        { "2023-09-07 12:36:06.543", new DateTime(2023, 9, 7, 12, 36, 6, 543) },
        { "2023-08-21 23:18:37.4321", new DateTime(2023, 8, 21, 23, 18, 37, 432, 100) },
        { "2024-05-13 23:18:15.999999", new DateTime(2024, 5, 13, 23, 18, 15, 999, 999) },
        { "2023-09-26 05:51:31", new DateTime(2023, 9, 26, 5, 51, 31) },
        { "2024-05-08 08:39:58", new DateTime(2024, 5, 8, 8, 39, 58) },
        { "2024-04-04 23:07:49.9", new DateTime(2024, 4, 4, 23, 7, 49, 900) },
        { "2024-07-04 08:37:30.87", new DateTime(2024, 7, 4, 8, 37, 30, 870) },
        { "2024-02-28 22:56:13.765", new DateTime(2024, 2, 28, 22, 56, 13, 765) },
        { "8/10/2024 13:14:15", new DateTime(2024, 8, 10, 13, 14, 15) },
        { "1/1/2024 12:00:00.1", new DateTime(2024, 1, 1, 12, 0, 0, 100) },
        { "12/31/2023 11:59:59.324 PM", new DateTime(2023, 12, 31, 23, 59, 59, 324) },
        { "12/31/2023 11:59:59.339 PM", new DateTime(2023, 12, 31, 23, 59, 59, 339) },
        { "08-05-98 3:34:10.001 PM", new DateTime(1998, 8, 5, 15, 34, 10, 1) },
        { "7/6/2024 5:05:17.321", new DateTime(2024, 7, 6, 5, 5, 17, 321) },
        { "8/5/98 3:34:10.999 PM", new DateTime(1998, 8, 5, 15, 34, 10, 999) },
        { "8-10-2024 3:04:05 AM", new DateTime(2024, 8, 10, 3, 4, 5) },
        { "8/10/24 1:14:15.000001 PM", new DateTime(2024, 8, 10, 13, 14, 15, 0, 1) },
        { "7/6/24 5:04:03.000123", new DateTime(2024, 7, 6, 5, 4, 3, 0, 123) },
        { "06-23-2024 00:58:45", new DateTime(2024, 6, 23, 0, 58, 45) },
        { "06-23-2024 00:58:45.309", new DateTime(2024, 6, 23, 0, 58, 45, 309) },
        { "06-23-2024 01:58 AM", new DateTime(2024, 6, 23, 1, 58, 0) },
        { "06-23-2024 01:58 Am", new DateTime(2024, 6, 23, 1, 58, 0) },
        { "06-23-2024 01:58 PM", new DateTime(2024, 6, 23, 13, 58, 0) },
        { "06-23-2024 01:58 Pm", new DateTime(2024, 6, 23, 13, 58, 0) },
        { "06-23-2024 01:58 aM", new DateTime(2024, 6, 23, 1, 58, 0) },
        { "06-23-2024 01:58 am", new DateTime(2024, 6, 23, 1, 58, 0) },
        { "06-23-2024 01:58 pM", new DateTime(2024, 6, 23, 13, 58, 0) },
        { "06-23-2024 01:58 pm", new DateTime(2024, 6, 23, 13, 58, 0) },
        { "2024/06/23 00:58:45", new DateTime(2024, 6, 23, 0, 58, 45) },
        { "2024/06/23 00:58:45.309", new DateTime(2024, 6, 23, 0, 58, 45, 309) },
        { "Sep 25 2025", new DateTime(2025, 9, 25, 0, 0, 0) },
        // JavaScript
        {"Wed Aug 05 00:00:00 CDT 1998", new DateTime(1998, 8, 5, 0, 0, 0) },
        {"Sat Oct 11 17:45:20 CDT 2008", new DateTime(2008, 10, 11, 17, 45, 20)},
        {"Wed Oct 19 16:03:29 GMT-05:00 1988", new DateTime(1988, 10, 19, 16, 3, 29)},
        {"Thu Sep 5 14:12:43 UTC-05:00 1968", new DateTime(1968, 9, 5, 14, 12, 43)},
        // locale-dependent
        { "Mar 30 2020", new DateTime(2020, 3, 30, 0, 0, 0) }, // abbreviated, non-recent
        { "Mar 30 23:45" , new DateTime(DateTime.Now.Year, 3, 30, 23, 45, 0) }, // abbreviated, recent
        { "07/25/2025 05:17 PM", new DateTime(2025, 7, 25, 17, 17, 0) }, // US locale
        { "25.07.2025 17:17", new DateTime(2025, 7, 25, 17, 17, 0) }, // European locale
        // timestamps
        { "20250725173754", new DateTime(2025, 7, 25, 17, 37, 54) },
        { "20250417", new DateTime(2025, 4, 17) },
    };

    [Theory]
    [MemberData(nameof(DateTimes))]
    public void ParsesDateTime(string input, DateTime expected)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.Equal(expected, ValueConverter.ParseDateTime(input));
    }

    [Theory]
    [MemberData(nameof(DateTimes))]
    public void ParsesNullableDateTime(string input, DateTime expected)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.Equal(expected, ValueConverter.ParseNullableDateTime(input)!.Value);
    }
    #endregion

    #region datetime offset
    [Theory]
    [MemberData(nameof(DateTimes))]
    public void ParsesDateTimeOffset(string input, DateTime expected)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.Equal(expected, ValueConverter.ParseDateTimeOffset(input, null));
    }

    [Theory]
    [MemberData(nameof(DateTimes))]
    public void TriesToParsesDateTimeOffset(string input, DateTime _)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.True(ValueConverter.TryParseDateTimeOffset(input, out DateTimeOffset _, null));
    }

    [Fact]
    public void FailsToParsesDateTimeOffset()
    {
        var dt = new DateTimeOffset(DateTime.Now);
        var d0 = new DateTimeOffset();

        Assert.False(ValueConverter.TryParseDateTimeOffset("input", out DateTimeOffset _, null));
        Assert.False(ValueConverter.TryParseDateTimeOffset(" ", out DateTimeOffset _, null));

        Assert.False(ValueConverter.TryParseDateTimeOffset(" ", out DateTimeOffset dto, dt));
        Assert.Equal(dt, dto);

        Assert.False(ValueConverter.TryParseDateTimeOffset("0", out dto, d0));
        Assert.Equal(d0, dto);

        Assert.True(ValueConverter.TryParseNullableDateTimeOffset("1968-04-27", out var ndto));
        Assert.Equal(new DateTime(1968, 4, 27), ndto);

        Assert.False(ValueConverter.TryParseNullableDateTimeOffset("25-32-1999", out ndto));
        Assert.Null(ndto);

        Assert.False(ValueConverter.TryParseNullableDateTimeOffset(null!, out ndto));
        Assert.Null(ndto);
    }

    [Theory]
    [MemberData(nameof(DateTimes))]
    public void ParsesNullableDateTimeOffset(string input, DateTime expected)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.Equal(expected, ValueConverter.ParseNullableDateTimeOffset(input)!.Value);
    }
    #endregion

    #region date
    public static TheoryData<string, DateOnly> Dates => new()
    {
        { "2024-08-09", new DateOnly(2024, 8, 9) },
        { "2023-12-02", new DateOnly(2023, 12, 2) },
        { "2024-06-06", new DateOnly(2024, 6, 6) },
        { "2023-11-29", new DateOnly(2023, 11, 29) },
        { "2024-04-01", new DateOnly(2024, 4, 1) },
        { "2024-06-26", new DateOnly(2024, 6, 26) },
        { "2023-10-20", new DateOnly(2023, 10, 20) },
        { "2024-06-29", new DateOnly(2024, 6, 29) },
        { "2023-09-18", new DateOnly(2023, 9, 18) },
        { "2023-11-16", new DateOnly(2023, 11, 16) },
        { "8/10/2024", new DateOnly(2024, 8, 10) },
        { "1/1/2024", new DateOnly(2024, 1, 1) },
        { "12/31/2023", new DateOnly(2023, 12, 31) },
        { "12/31/2023", new DateOnly(2023, 12, 31) },
        { "08-05-98", new DateOnly(1998, 8, 5) },
        { "7/6/2024", new DateOnly(2024, 7, 6) },
        { "8/5/98", new DateOnly(1998, 8, 5) },
        { "8-10-2024", new DateOnly(2024, 8, 10) },
        { "8/10/24", new DateOnly(2024, 8, 10) },
        { "7/6/24", new DateOnly(2024, 7, 6) },
        { "06-23-2024", new DateOnly(2024, 6, 23) },
        { "2024/06/23", new DateOnly(2024, 6, 23) },
        { "2024/06/23", new DateOnly(2024, 6, 23) },
        { "Sep 25 2025", new DateOnly(2025, 9, 25) }
    };

    [Theory]
    [MemberData(nameof(Dates))]
    public void ParsesDate(string input, DateOnly expected)
    {
        Assert.Equal(expected, ValueConverter.ParseDate(input));
    }

    public static TheoryData<string?, DateOnly, string, DateOnly> DatesWithOptions => new()
    {
        { "25/12/2025", new DateOnly(1970, 1, 1), "fr-FR", new DateOnly(2025, 12, 25) }
    };

    [Theory]
    [MemberData(nameof(DatesWithOptions))]
    public void ParsesDateWithOptions(string? obj, DateOnly defaultValue, string codePage, DateOnly expected)
    {
        Assert.Equal(expected, ValueConverter.Parse(obj!, typeof(DateOnly), defaultValue, ValueConverter.ParseCultureInfo(codePage)));
    }

    [Fact]
    public void GenericParsesDateOrThrows()
    {
        Assert.Equal(new DateOnly(2024, 8, 9), ValueConverter.Parse<DateOnly>("2024-08-09"));
        Assert.Throws<FormatException>(() => ValueConverter.Parse<DateOnly>("2024-31-27"));
    }

    [Fact]
    public void TriesToParseDate()
    {
        Assert.True(ValueConverter.TryParse("2024-08-09", out DateOnly _));
        Assert.False(ValueConverter.TryParse("2024-31-27", out DateOnly _));
    }

    public static TheoryData<string?, string?, DateTime?, TrimmingOptions, bool> NullableDates => new()
    {
        { "Sat Apr 27 03:15:00 CDT 1968", null, new DateTime(1968, 4, 27, 3, 15, 0), TrimmingOptions.Both, true },
        { "   Sat Apr 27 03:15:00 CDT 1968", null, null, TrimmingOptions.None, false },
        { "                               ", null, null, TrimmingOptions.Start, false }
    };

    [Theory]
    [MemberData(nameof(NullableDates))]
    public void TriesToParseNullableDates(string? input, string? codePage, DateTime? expected, TrimmingOptions trimmingOptions, bool result)
    {
        Assert.Equal(result, ValueConverter.TryParseNullableDateTime(input!, out var value, ValueConverter.ParseCultureInfo(codePage), trimmingOptions));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void TriesToParseTimeSpan()
    {
        Assert.True(ValueConverter.TryParse("23:59:59", out TimeSpan _));
        Assert.False(ValueConverter.TryParse("25:25:25", out TimeSpan _));
    }

    public static TheoryData<string?, string?, TimeSpan?, TrimmingOptions, bool> NullableTimeSpans => new()
    {
        { "7.00:00:07", null, new TimeSpan(7, 0, 0, 7), TrimmingOptions.None, true },
        { "X.99:00:00", null, TimeSpan.MinValue, TrimmingOptions.Both, false },
        { "  ", null, TimeSpan.MinValue, TrimmingOptions.Both, false }
    };


    [Theory]
    [MemberData(nameof(NullableTimeSpans))]
    public void TriesToParseNullableTimeSpans(string? input, string? codePage, TimeSpan? expected, TrimmingOptions trimmingOptions, bool result)
    {
        Assert.Equal(result, ValueConverter.TryParseNullableTimeSpan(input!, out var value, culture: ValueConverter.ParseCultureInfo(codePage), trimmingOptions: trimmingOptions));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void ParsesNullableTimeSpans()
    {
        Assert.Equal(new TimeSpan(2, 12, 30, 15), ValueConverter.ParseNullableTimeSpan("2.12:30:15"));
    }

    #endregion

    #region time
    public static TheoryData<string, TimeOnly> Times => new()
    {
        { "03:25:05.2456256", new TimeOnly(123052456256) },
        { "02:39:14.345", new TimeOnly(2, 39, 14, 345) },
        { "07:47:55.5676", new TimeOnly(7, 47, 55, 567, 600) },
        { "11:45:55.5464", new TimeOnly(11, 45, 55, 546, 400) },
        { "03:58:25.134", new TimeOnly(3, 58, 25, 134) },
        { "09:41:24.00001", new TimeOnly(9, 41, 24, 0, 10) },
        { "02:40:13.0001", new TimeOnly(2, 40, 13, 0, 100) },
        { "13:21:25.999", new TimeOnly(13, 21, 25, 999) },
        { "17:18:55.88", new TimeOnly(17, 18, 55, 880) },
        { "02:33:52.7", new TimeOnly(2, 33, 52, 700) },
        { "17:55:04.65", new TimeOnly(17, 55, 4, 650) },
        { "12:36:06.543", new TimeOnly(12, 36, 6, 543) },
        { "23:18:37.4321", new TimeOnly(23, 18, 37, 432, 100) },
        { "23:18:15.999999", new TimeOnly(23, 18, 15, 999, 999) },
        { "05:51:31", new TimeOnly(5, 51, 31) },
        { "08:39:58", new TimeOnly(8, 39, 58) },
        { "23:07:49.9", new TimeOnly(23, 7, 49, 900) },
        { "08:37:30.87", new TimeOnly(8, 37, 30, 870) },
        { "22:56:13.765", new TimeOnly(22, 56, 13, 765) },
        { "22.56.13", new TimeOnly(22, 56, 13) }
    };

    [Theory]
    [MemberData(nameof(Times))]
    public void ParseTime(string input, TimeOnly expected)
    {
        ValueConverter.AddTimeFormat("HH.mm.ss");

        Assert.Equal(expected, ValueConverter.ParseTime(input));
    }

    [Fact]
    public void TriesToParseTime()
    {
        Assert.False(ValueConverter.TryParseTime("   ", out var _));
    }

    [Fact]
    public void TriesToParseNullableTime()
    {
        Assert.False(ValueConverter.TryParseNullableTime("   ", out var _));
        Assert.False(ValueConverter.TryParseNullableTime("   ", out var _, new TimeOnly(12, 0), trimmingOptions: TrimmingOptions.None));

        Assert.True(ValueConverter.TryParseNullableTime("07:06:42", out var _));
    }

    [Fact]
    public void ParsesNullableTimeOrThrows()
    {
        Assert.Equal(new TimeOnly(5, 1), ValueConverter.ParseNullableTime("05:01", new TimeOnly(23, 0)));
        Assert.Throws<FormatException>(() => ValueConverter.ParseNullableTime(" "));
    }

    #endregion

    #region timespan
    public static TheoryData<string, TimeSpan, string, TrimmingOptions, TimeSpan> TimeSpans => new()
    {
        { "03:30:33", new TimeSpan(0, 0, 0), "", TrimmingOptions.Both, new TimeSpan(3, 30, 33) }
    };

    [Theory]
    [MemberData(nameof(TimeSpans))]
    public void ParsesTimespan(string input, TimeSpan defaultValue, string codePage, TrimmingOptions trimmingOptions, TimeSpan expected)
    {
        Assert.Equal(expected, ValueConverter.ParseTimeSpan(input, defaultValue, ValueConverter.ParseCultureInfo(codePage), trimmingOptions));
    }

    public static TheoryData<string, string, TrimmingOptions, bool> TryTimeSpans => new()
    {
        { "03:30:33", "", TrimmingOptions.None, true },
        { " 15:45:32", "", TrimmingOptions.Both, true },
        { "  ", "", TrimmingOptions.Both, false }
    };

    [Theory]
    [MemberData(nameof(TryTimeSpans))]
    public void TriesToParseTimespan(string input, string codePage, TrimmingOptions trimmingOptions, bool result)
    {
        Assert.Equal(result, ValueConverter.TryParseTimeSpan(input, out _, culture: ValueConverter.ParseCultureInfo(codePage), trimmingOptions: trimmingOptions));
        Assert.Equal(result, ValueConverter.TryParseTimeSpan(input, out _));
    }
    #endregion

    #region DBNull
    [Fact]
    public void ParsesDBNull()
    {
        var type = typeof(bool);

        Assert.Null(ValueConverter.Parse(DBNull.Value, type));
        Assert.Equal(false, ValueConverter.Parse(DBNull.Value, type, false));
        Assert.Equal(true, ValueConverter.Parse(DBNull.Value, type, true));
    }
    #endregion

    #region double
    public static TheoryData<string?, double, string?, double?> DoubleStrings => new()
    {
        { "12.34", 0.0, null, 12.34 },
        { "12,34", 0.0, "de-DE", 12.34 }
    };

    [Theory]
    [MemberData(nameof(DoubleStrings))]
    public void ParsesDouble(string? obj, double defaultValue, string? codePage, double? expected)
    {
        var result = codePage is not null
            ? ValueConverter.Parse(obj!, typeof(double), defaultValue, ValueConverter.ParseCultureInfo(codePage))
            : ValueConverter.Parse(obj!, typeof(double), defaultValue);

        Assert.Equal(expected, result);
    }
    #endregion

    #region string
    public static TheoryData<string?, string?, string?, string?> Strings => new()
    {
        { "lorem ipsum", null, "fr-FR", "lorem ipsum" }
    };

    [Theory]
    [MemberData(nameof(Strings))]
    public void ParsesString(string? obj, string? defaultValue, string? codePage, string? expected)
    {
        var result = codePage is not null
            ? ValueConverter.Parse(obj!, typeof(string), defaultValue, ValueConverter.ParseCultureInfo(codePage))
            : ValueConverter.Parse(obj!, typeof(string), defaultValue);

        Assert.Equal(expected, result);
    }
    #endregion

    #region decimals
    public static TheoryData<string?, decimal?, string?, decimal?> DecimalStrings => new()
    {
        { "12", -1m, null, 12m }
    };

    [Theory]
    [MemberData(nameof(DecimalStrings))]
    public void ParsesDecimal(string? obj, decimal? defaultValue, string? codePage, decimal? expected)
    {
        var result = codePage is not null
            ? ValueConverter.Parse(obj!, typeof(decimal), defaultValue, ValueConverter.ParseCultureInfo(codePage))
            : ValueConverter.Parse(obj!, typeof(decimal), defaultValue);

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

    [Fact]
    public void TriesToParse()
    {
        Assert.False(ValueConverter.TryParse(null!, out var value1, -1m, null));
        Assert.Equal(-1m, value1);

        Assert.False(ValueConverter.TryParse("", out decimal value2));
        Assert.Equal(0m, value2);

        Assert.True(ValueConverter.TryParse("23.42", out decimal _));

        Assert.True(ValueConverter.TryParse("end", out TrimmingOptions _));
        Assert.True(ValueConverter.TryParse("e9cc294d-0a31-481b-bc61-f677c1392516", out Guid _));
        Assert.True(ValueConverter.TryParse("23:15:49", out TimeOnly _));
        Assert.True(ValueConverter.TryParse("23:15:49", out DateTime _));

        Assert.True(ValueConverter.TryParse("2345098756589879898", out long _));
        Assert.True(ValueConverter.TryParse("11.33", out float _));
        Assert.True(ValueConverter.TryParse("234", out short _));
        Assert.True(ValueConverter.TryParse("253", out byte _));
        Assert.True(ValueConverter.TryParse("-128", out sbyte _));
        Assert.True(ValueConverter.TryParse("67", out ushort _));
        Assert.True(ValueConverter.TryParse("32454", out uint _));
        Assert.True(ValueConverter.TryParse("18446744073709551615", out ulong _));
        Assert.True(ValueConverter.TryParse("c", out char _));

        Assert.False(ValueConverter.TryParse("extra", out TrimmingOptions _));
        Assert.False(ValueConverter.TryParse("481bf677c1392516", out Guid _));
        Assert.False(ValueConverter.TryParse("65:78:99", out TimeOnly _));
        Assert.False(ValueConverter.TryParse("31-12-1921", out DateTime _));

        Assert.False(ValueConverter.TryParse("NotANumber", out long _));
        Assert.False(ValueConverter.TryParse("NaN!", out float _));
        Assert.False(ValueConverter.TryParse("what?", out short _));
        Assert.False(ValueConverter.TryParse("me", out byte _));
        Assert.False(ValueConverter.TryParse("-=1", out sbyte _));
        Assert.False(ValueConverter.TryParse("no, u short", out ushort _));
        Assert.False(ValueConverter.TryParse("I'm in", out uint _));
        Assert.False(ValueConverter.TryParse("you know it", out ulong _));
        Assert.False(ValueConverter.TryParse("cc", out char _));

        Assert.Throws<NotSupportedException>(() => ValueConverter.TryParse("55", out ValueType value3, -1m, null));

        Assert.Throws<FormatException>(() => ValueConverter.ParseChar("obj"));
        Assert.Equal('x', ValueConverter.ParseChar("x"));
        Assert.False(ValueConverter.TryParseChar("obj", out char _));
        Assert.False(ValueConverter.TryParseChar(string.Empty, out char _));

        Assert.False(ValueConverter.TryParseNullableDecimal("d", out decimal? d));
        Assert.Null(d);

        Assert.False(ValueConverter.TryParse($"{null}", out int? y));
        Assert.Null(y);

        Assert.False(ValueConverter.TryParse("", typeof(string), out var s, "<empty>", CultureInfo.GetCultureInfo("fr-FR")));
        Assert.Equal("<empty>", s);

        Assert.False(ValueConverter.TryParse(null!, typeof(string), out s, "<empty>"));
        Assert.Equal("<empty>", s);

        Assert.True(ValueConverter.TryParse("hello", typeof(string), out s, "<empty>"));
        Assert.Equal("hello", s);

        Assert.False(ValueConverter.TryParseNullableBoolean("2", out var v));
        Assert.Null(v);
        Assert.False(ValueConverter.TryParseNullableBoolean("F", out v));
        Assert.Null(v);
        Assert.False(ValueConverter.TryParseNullableBoolean("-1", out v));
        Assert.Null(v);

        Assert.False(ValueConverter.TryParseNullableEnum("extra", out TrimmingOptions? options));
        Assert.Null(options);

        Assert.Equal(TrimmingOptions.Both, ValueConverter.ParseEnum(typeof(TrimmingOptions), "BOTH    ", TrimmingOptions.None, TrimmingOptions.End));
        Assert.False(ValueConverter.TryParseEnum(typeof(TrimmingOptions), " BOTH ", out var t, TrimmingOptions.None, TrimmingOptions.None));
        Assert.False(ValueConverter.TryParseEnum(typeof(TrimmingOptions), "      ", out t, TrimmingOptions.None, TrimmingOptions.Both));
        Assert.Throws<FormatException>(() => ValueConverter.ParseEnum(typeof(TrimmingOptions), " BOTH ", TrimmingOptions.None, TrimmingOptions.None));

        Assert.Equal(new Guid("93184298-7d9c-4483-b664-0fa60f28f812"), ValueConverter.ParseGuid("93184298-7d9c-4483-b664-0fa60f28f812", Guid.Empty));
        Assert.False(ValueConverter.TryParseGuid("", out var g, Guid.Empty, TrimmingOptions.None));
        Assert.False(ValueConverter.TryParseGuid("notaguid", out g, Guid.Empty, TrimmingOptions.None));
        Assert.False(ValueConverter.TryParseNullableGuid("", out Guid? g1, TrimmingOptions.None));
        Assert.False(ValueConverter.TryParseNullableGuid("93184298-7d9x-4483-b664-0fa60f28f812", out Guid? g2, TrimmingOptions.None));
        Assert.Equal(new Guid("93184298-7d9c-4483-b664-0fa60f28f812"), ValueConverter.ParseNullableGuid("93184298-7d9c-4483-b664-0fa60f28f812", TrimmingOptions.None));

        Assert.True(ValueConverter.TryParseDateTime("2024-08-09", out var dt));
        Assert.Equal(new DateTime(2024, 8, 9), dt);

        Assert.False(ValueConverter.TryParseDateTime("2024-31-27", out dt));
        Assert.Equal(DateTime.MinValue, dt);

        Assert.False(ValueConverter.TryParseDate("", out var d1));
        Assert.False(ValueConverter.TryParseDate("", out d1, new DateOnly(1970, 1, 1)));
        Assert.Equal(1970, d1.Year);

        //Assert.True(ValueConverter.TryParseDate("Sat Apr 27 03:15:00 GMT-04:30 1968", out d1));
        //Assert.True(ValueConverter.TryParseDate("Sat Apr 27 03:15:00 UTC-00:00 1968", out d1));
        Assert.True(ValueConverter.TryParseDate("Sat Apr 27 03:15:00 CDT 1968", out d1));
        Assert.Equal(d1, ValueConverter.ParseNullableDate("Sat Apr 27 03:15:00 CDT 1968")!.Value);
        Assert.False(ValueConverter.TryParseDate("NotADate", out d1));
        Assert.False(ValueConverter.TryParseNullableDate(" ", out DateOnly? d2));

        Assert.True(ValueConverter.TryParse("12-31-1291", out DateTimeOffset _));
        Assert.False(ValueConverter.TryParse("31-12-1921", out DateTimeOffset _));

    }
}
