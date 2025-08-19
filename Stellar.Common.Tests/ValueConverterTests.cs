using System.Globalization;

namespace Stellar.Common.Tests;

public class ValueConverterTests
{
    #region data
    public static TheoryData<string> Formats =>
    [
        "yyyy-MM-dd",
        "HH.mm.ss"
    ];

    #region boolean
    public static TheoryData<string, bool> TruthyAndFalsy => new()
    {
        { "True", true },
        { " true", true },
        { "TRUE ", true },
        { "False", false },
        { " false", false },
        { "FALSE ", false },
        { "0", false },
        { " 0 ", false },
        { "1", true },
        { " 1 ", true },
        { "", false },
        { null!, false }
    };

    public static TheoryData<string, bool?> NullableTruthyAndFalsy => new()
    {
        { "True", true },
        { "FALSE ", false },
        { "0", false },
        { "1", true },
        { "", null }
    };

    public static TheoryData<string> NotBoolean => new()
    {
        { "YES" },
        { "NO" },
        { "ON" },
        { "OFF" },
        { "T" },
        { "F" },
        { "2" },
        { "-1" }
    };
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
    #endregion

    #region enum
    public static TheoryData<string, TrimmingOptions> Enum1 => new()
    {
        { "   Both   ", TrimmingOptions.Both },
        { "Start ", TrimmingOptions.Start },
        { "   End", TrimmingOptions.End },
        { "None", TrimmingOptions.None },
        { "", TrimmingOptions.None }
    };

    public static TheoryData<string, TrimmingOptions?> Enum2 => new()
    {
        { "   Both   ", TrimmingOptions.Both },
        { "Start ", TrimmingOptions.Start },
        { "   End", TrimmingOptions.End },
        { "None", TrimmingOptions.None },
        { "", null }
    };
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
        { "Sep 25 2025", new DateOnly(2025, 9, 25) },
    };
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
    #endregion

    #region object
    public static TheoryData<object?, Type, object?> Objects => new()
    {
        { true, typeof(bool), true },
        { false, typeof(bool), false },
        { DBNull.Value, typeof(bool), null },
    };

    public static TheoryData<object?, Type, object?, IFormatProvider?, object?> ObjectsWithOptions => new()
    {
        { true, typeof(bool), null, null, true },
        { false, typeof(bool), null, null, false },
        { DBNull.Value, typeof(bool), null, null, null },
        { DBNull.Value, typeof(bool), false, null, false },

        { 12.34, typeof(double), 0.0, null, 12.34 },
        { "12,34", typeof(double), 0.0, CultureInfo.GetCultureInfo("de-DE"), 12.34 },
        { "25/12/2025", typeof(DateOnly), null, CultureInfo.GetCultureInfo("fr-FR"), new DateOnly(2025, 12, 25) }
    };

    #endregion

    #region to string
    public static TheoryData<object?, string, string?, IFormatProvider?> ToStringData => new()
    {
        { true, "True", null, null },
        { false, "False", null, null },
        { "", "", null, null },
        { null !, "", null, null },

        { new DateTime(2025, 7, 18), "07/18/2025 00:00:00", null, null },
        { new DateTime(2025, 7, 18, 20, 45, 32), "2025-Jul-18 20:45:32", "yyyy-MMM-dd HH:mm:ss", null },
        
        { DateTimeOffset.FromUnixTimeSeconds(7), "00:00:07", "HH:mm:ss", null },
        
        { new DateOnly(2025, 7, 18), "07/18/2025", null, null },
        { new DateOnly(2025, 7, 18), "2025-Jul-18", "yyyy-MMM-dd", null },
        
        { new TimeOnly(20, 45, 32), "20:45", null, null },
        
        { new TimeSpan(8, 45, 32), "08:45:32", null, CultureInfo.GetCultureInfo("fi-FI") },
        { new TimeSpan(8, 45, 32), "08:45:32", null, null },

        { 123.456, "123.456", null, null },
        { 123.456, "123,456", null, CultureInfo.GetCultureInfo("de-DE") },
        { 'A', "A", null, null },
        { ' ', " ", null, null },
        { '\u2192', "\u2192", null, null },
        { 1.5E-5d, "1.5E-05", null, null },
        { 1.5E-5d, "0.0000150000", "N10", null },
        { 1.54321E-3f, "1.54321E-003", "E5", null },
        { 1987.6m, "$1,987.60", "C2", CultureInfo.GetCultureInfo("en-US") },

        { 123456, "123,456", "N0", null },
        { 123456, "123.456", "N0", CultureInfo.GetCultureInfo("de-DE") },

        { 123456L, "123,456", "N0", null },
        { 123456L, "123.456", "N0", CultureInfo.GetCultureInfo("de-DE") },

        { (short)12356, "12,356", "N0", null },

        { 1234567890121UL, "1,234,567,890,121", "N0", null },
        { 75849584532321U, "75 849 584 532 321", "### ### ### ### ###", null },

        { (byte)128, "80", "X2", null },
        { (sbyte)-80, "B0", "X2", null },

        { (uint)256, "100", "X2", null },
        { (ushort)80, "50", "X2", null },

        { true,  "True", "'Y','N'", null }, // format ignored...
        { false, "False", "'Y','N'", null },

        {  new Guid("e9cc294d-0a31-481b-bc61-f677c1392516"), "{0xe9cc294d,0x0a31,0x481b,{0xbc,0x61,0xf6,0x77,0xc1,0x39,0x25,0x16}}", "X", null },
        { Guid.Empty, "(00000000-0000-0000-0000-000000000000)", "P", null },
        
        { new DateTime(2025, 7, 18), "7/18/2025 12:00:00 AM", null, CultureInfo.GetCultureInfo("en-US") },
        { new DateTime(2025, 12, 31), "2025-Dec-31 00:00:00", "yyyy-MMM-dd HH:mm:ss", CultureInfo.GetCultureInfo("en-US") },

        { new DateOnly(2025, 7, 18), "07/18/2025", null, null },
        { new DateOnly(2025, 7, 18), "2025-Jul-18", "yyyy-MMM-dd", null },

        { new TimeOnly(20, 45, 32), "08.45.32", "hh.mm.ss", null },
        
        { new TimeSpan(8, 45, 32), "08:45:32", null, CultureInfo.GetCultureInfo("fi-FI") },
        { new TimeSpan(8, 45, 32), "08:45:32", null, null },

        { 123.456, "123.456", null, null },
        { 123.456, "123,456", null, CultureInfo.GetCultureInfo("de-DE") },
        { 'A', "A", null, null },
        { ' ', " ", null, null },
        { '\u2192', "\u2192", null, null },
        { 1.5E-5d, "1.5E-05", null, null },

        { new Bucket<string>(["a", "b"]), "a:0 b:0", null, null },
    };
    #endregion
    #endregion

    [Theory]
    [MemberData(nameof(Formats))]
    public void SetsAndGetsDateTimeFormats(string format)
    {
        ValueConverter.AddDateTimeFormat(format);
        ValueConverter.AddDateTimeFormats([format]);

        var formats = ValueConverter.GetDateTimeFormats();

        Assert.Contains(format, formats);
    }

    [Theory]
    [MemberData(nameof(ToStringData))]
    public void ConvertsToString(object? obj, string expected, string? format, IFormatProvider? culture)
    {
        Assert.Equal(expected, ValueConverter.ToString(obj!, format, culture: culture));
    }

    [Theory]
    [MemberData(nameof(TruthyAndFalsy))]
    public void ToBoolean(string input, bool expected)
    {
        Assert.Equal(expected, ValueConverter.ParseBoolean(input));
    }


    [Theory]
    [MemberData(nameof(NullableTruthyAndFalsy))]
    public void ToNullableBoolean(string input, bool? expected)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableBoolean(input));
    }

    [Theory]
    [MemberData(nameof(NotBoolean))]
    public void ToBooleanThrows(string input)
    {
        Assert.Throws<FormatException>(() => ValueConverter.ParseBoolean(input));
    }

    [Theory]
    [MemberData(nameof(Chars))]
    public void ToNullableChar(string input, char? expected, TrimmingOptions trimmingOptions)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableChar(input!, trimmingOptions));
    }

    [Theory]
    [MemberData(nameof(Enum1))]
    public void ToEnum(string input, TrimmingOptions expected)
    {
        Assert.Equal(expected, ValueConverter.ParseEnum<TrimmingOptions>(input));
    }

    [Theory]
    [MemberData(nameof(Enum2))]
    public void ToNullableEnum(string input, TrimmingOptions? expected)
    {
        Assert.Equal(expected, ValueConverter.ParseNullableEnum<TrimmingOptions>(input));
    }

    [Fact]
    public void ToEnumThrows()
    {
        Assert.Throws<FormatException>(() => ValueConverter.ParseEnum("bogus", TrimmingOptions.None));
    }

    [Theory]
    [MemberData(nameof(DateTimes))]
    public void ToDateTime(string input, DateTime expected)
    {
        ValueConverter.AddDateTimeFormats([
            "MMM dd HH:mm",
            "dd'.'MM'.'yyyy HH:mm"
        ]);

        Assert.Equal(expected, ValueConverter.ParseDateTime(input));
    }

    [Theory]
    [MemberData(nameof(Dates))]
    public void ToDate(string input, DateOnly expected)
    {
        Assert.Equal(expected, ValueConverter.ParseDate(input));
    }

    [Theory]
    [MemberData(nameof(Times))]
    public void ToTime(string input, TimeOnly expected)
    {
        var format = "HH.mm.ss";

        ValueConverter.AddTimeFormat(format);
        Assert.Contains(format, ValueConverter.GetTimeFormats());

        Assert.Equal(expected, ValueConverter.ParseTime(input));
    }

    [Theory]
    [MemberData(nameof(Objects))]
    public void FromObject(object? input, Type type, object? expected)
    {
        var value = ValueConverter.Parse(input, type);

        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("2024-08-09")]
    public void GenericParse(string input)
    {
        var value1 = ValueConverter.Parse<DateOnly>(input);
        
        Assert.True(ValueConverter.TryParse(input, out DateOnly value2));
        
        Assert.Equal(new DateOnly(2024, 8, 9), value1);
        Assert.Equal(new DateOnly(2024, 8, 9), value2);
    }

    [Theory]
    [InlineData("2024-31-27")]
    public void GenericParseFails(string input)
    {
        Assert.Throws<FormatException>(() => ValueConverter.Parse<DateOnly>(input));
        
        Assert.False(ValueConverter.TryParse(input, out DateOnly value2));
        
        Assert.Equal(DateOnly.MinValue, value2);
    }

    [Theory]
    [MemberData(nameof(ObjectsWithOptions))]
    public void FromObjectWithOptions(object? input, Type type, object? defaultValue, IFormatProvider? culture, object? expected)
    {
        var value = ValueConverter.Parse(input, type, defaultValue, culture);

        Assert.Equal(expected, value);
    }
}
