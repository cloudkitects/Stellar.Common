namespace Stellar.Common.Tests;

/// <remarks>
/// <see cref="ValueConverter.IntegerNumberStyles"/> does not include allowing
/// parenthesis by default, but it is under user control.
/// Thousands separator allowed by default.
/// </remarks>
public partial class ValueConverterTests
{
    public static TheoryData<string, sbyte?, string?, bool, sbyte?> SignedByteData => new()
    {
        { "  127  ", null, null,  true,   127 },
        { " -128 ",  null, null,  true,  -128 },
        { "  +12 ",  null, null,  true,    12 },
        { "  +12 ",    42, null,  true,    12 },
        { "   12 ",    42, null,  true,    12 },
        { "   12 ",  null, null,  true,    12 },
        { "     ",   null, null, false,     0 },
        { "     ",     42, null, false,    42 },
        { null!,     null, null, false,     0 },
        { "  +130",    42, null, false,    42 },
        { "  -130",    42, null, false,    42 }
    };

    public static TheoryData<string, byte?, string?, bool, byte?> ByteData => new()
    {
        { "  255  ", null, null,  true,   255 },
        { "   12 ",  null, null,  true,    12 },
        { "   12 ",    42, null,  true,    12 },
        { "   12 ",  null, null,  true,    12 },
        { "     ",   null, null, false,     0 },
        { "     ",     42, null, false,    42 },
        { null!,     null, null, false,     0 },
        { "  +260",    42, null, false,    42 },
        { "   -5",     42, null, false,    42  }
    };

    public static TheoryData<string, short?, string?, bool, short?> ShortData => new()
    {
        { "  32,767  ", null, null,    true,  32767 },
        { " -32,768 ",  null, null,    true, -32768 },
        { "  +1234 ",   null, null,    true,   1234 },
        { "  +1234 ",     42, null,    true,   1234 },
        { "   1234 ",     42, null,    true,   1234 },
        { "   1234 ",   null, null,    true,   1234 },
        { "     ",      null, null,   false,      0 },
        { "     ",        42, null,   false,     42 },
        { null!,        null, null,   false,      0 },
        { "  +33000",     42, null,   false,     42 },
        { "  -33000",     42, null,   false,     42 }
    };

    public static TheoryData<string, ushort?, string?, bool, ushort?> UnsignedShortData => new()
    {
        { "  65,535  ", null, null,    true,  65535 },
        { "   1234 ",   null, null,    true,   1234 },
        { "   1234 ",     42, null,    true,   1234 },
        { "   1234 ",   null, null,    true,   1234 },
        { "     ",      null, null,   false,      0 },
        { "     ",        42, null,   false,     42 },
        { null!,        null, null,   false,      0 },
        { "  +66000",     42, null,   false,     42 },
        { "   -5",        42, null,   false,     42 }
    };

    public static TheoryData<string, int?, string?, bool, int> IntegerData => new()
    {
        { "67",         -1, null,    true,    67 },
        { "-67",        -1, null,    true,   -67 },
        { "(67)",       -1, null,    false,   -1 },
        { " 67 ",       -1, null,    true,    67 },
        { " 67 ",       -1, null,    true,    67 },
        { "    ",     null, null,    false,    0 },
        { "    ",       42, null,    false,   42 },
        { null!,      null, null,    false,    0 },
        { " 67.500 ",   -2, "de-DE", true, 67500 },
        { " 67,500 ",   -2, null,    true, 67500 },
        { " 67.50  ", null, "de-DE", true,  6750 }
    };

    public static TheoryData<string, uint?, string?, bool, uint> UnsignedIntegerData => new()
    {
        { "42",         67, null,    true,    42 },
        { " 42 ",       67, null,    true,    42 },
        { " 42 ",       67, null,    true,    42 },
        { "    ",     null, null,    false,    0 },
        { "    ",       67, null,    false,   67 },
        { null!,      null, null,    false,    0 },
        { " 42.670 ",    0, "de-DE", true, 42670 },
        { " 42,670 ",    0, null,    true, 42670 },
        { " 42.67  ", null, "de-DE", true,  4267 }
    };

    public static TheoryData<string, long?, string?, bool, long> LongIntegerData => new()
    {
        { " 2,147,483,648 ", null, null,     true,  2147483648L },
        { "-2,147,483,649 ",   -1, null,     true, -2147483649L },
        { "(2,147,483,649)",   -1, null,    false,          -1L },
        { " 2.147.483.648 ", null, "de-DE",  true,  2147483648L },
        { "-2.147.483.649 ",   -1, "de-DE",  true, -2147483649L },
        { "(2.147.483.649)",   -1, "de-DE", false,          -1L },
        { "    ",            null, null,    false,           0L },
        { "    ",              67, null,    false,          67L },
        { null!,             null, null,    false,           0L }
    };

    public static TheoryData<string, ulong?, string?, bool, ulong> UnsignedLongIntegerData => new()
    {
        { " 4,294,967,296 ",        null, null,     true,            4294967296UL },
        { " 67 ",                   42UL, "fr-FR",  true,                    67UL },
        { "42  ",                   null, "de-DE",  true,                    42UL },
        { "    ",                   42UL, null,    false,                    42UL },
        { null!,                    null, null,    false,                     0UL },
        { " 18446744073709551615 ", null, null,     true,  18446744073709551615UL },
        { "  18446744073709551616", 33UL, null,    false,                    33UL }
    };

    public static TheoryData<string, float?, string?, bool, float> FloatData => new()
    {
        { "   9.236784E-06", null, null,     true,   9.236784E-06F },
        { "      157639820", null, null,     true,      157639820F },
        { "  -4,415795E-20",  42F, "fr-FR",  true,  -4.415795E-20F },
        { "     -310607,06", null, "de-DE",  true,     -310607.06F },
        { "  1.2020839E-31",  42F, null ,    true,  1.2020839E-31F },
        { "       419.6408", null, null,     true,       419.6408F },
        { " -2.9579116E-34", null, null,     true, -2.9579116E-34F },
        { " -1.3747673E-19", null, null,     true, -1.3747673E-19F },
        { "       14033399", null, null,     true,       14033399F },
        { "     -1.2639792",  33F, null,     true,     -1.2639792F },
        { "    ",             42F, null,    false,             42F },
        { null!,             null, null,    false,              0F }
    };

    public static TheoryData<string, double?, string?, bool, double> DoubleData => new()
    {
        { "    -20856.546393160686", null, null,     true,     -20856.546393160686 },
        { " 2.6432229867387824E-29", null, null,     true,  2.6432229867387824E-29 },
        { "-1.3919780998960077E-36", null, null,     true, -1.3919780998960077E-36 },
        { "  1.098234855511675E-21", null, null,     true,   1.098234855511675E-21 },
        { "    -28911748367668.656", null, null,     true,     -28911748367668.656 },
        { "-3.6327098703367375E+36", null, null,     true, -3.6327098703367375E+36 },
        { "  7.967461929603659E-16", null, null,     true,   7.967461929603659E-16 },
        { " -1.079684599516832E-26", null, null,     true,  -1.079684599516832E-26 },
        { "      2.649066325878522", null, null,     true,       2.649066325878522 },
        { " -0.0008472728986549682", null, null,     true,  -0.0008472728986549682 },
        { "    ",                     42F, null,    false,                     42d },
        { null!,                     null, "de-DE", false,                      0d }
    };

    public static TheoryData<string, decimal?, string?, bool, decimal> DecimalData => new()
    {
        { "        ¤784,109.9525",   null,    null,  true,      784109.9525m },
        { "    (¤1,188,401.4588)",   null,    null,  true,    -1188401.4588m },
        { "(¤1,621,262,223.0987)",   null,    null,  true, -1621262223.0987m },
        { "            (¤2.4401)",   null,    null,  true,          -2.4401m },
        { "              ¤0.0000",   null,    null,  true,                0m },
        { "            (¤1.8096)",   null,    null,  true,          -1.8096m },
        { "      (¤899,328.9995)",   null,    null,  true,     -899328.9995m },
        { "               0.7500",   null,    null,  true,             0.75m },
        { "         ¤29,676.1557",   null,    null,  true,       29676.1557m },
        { "              ¤0.0001",   null,    null,  true,           0.0001m },
        { "    ",                  42.67m,    null, false,            42.67m },
        { null!,                     null, "de-DE", false,                0m },
        { "        $784,109.9525",   null, "en-US",  true,      784109.9525m },
        { "   (1 188 401,4588 €)",   null, "fr-FR",  true,    -1188401.4588m }
    };

    public static TheoryData<string, bool?, bool, bool> BooleanData => new()
    {
        { " true",  null,  true,  true },
        { "FALSE",  null,  true, false },
        { "     ",  null, false, false },
        { null!,    null, false, false },
        { "FALS ",  null, false, false },
        { "VRAI",  false, false, false },
        { "TRUE ",  null,  true,  true },
        { "fAlSe",  null,  true, false },
        { " true",  null,  true,  true },
        { "TRUth",  true, false,  true }
    };

    public static TheoryData<string, char?, TrimmingOptions, bool, char> CharData => new()
    {
        { " t ",  null, TrimmingOptions.Both,  true,  't' },
        { " t ",  null, TrimmingOptions.None, false,  default }
    };

    [Flags]
    public enum Enum1
    {
        Zero,
        One,
        Two,
        Three = One | Two
    };

    public static TheoryData<string, Enum1?, TrimmingOptions?, bool, Enum1?> EnumData => new()
    {
        { "   Zero   ",      null,  null,                  true,  Enum1.Zero  },
        { "    ONE   ",      null,  TrimmingOptions.Both,  true,  Enum1.One   },
        { "   Zero   ", Enum1.Two,  null,                  true,  Enum1.Zero  },
        { "   Zero   ", Enum1.Two,  TrimmingOptions.None,  false, Enum1.Two   },
        { "   two"    , Enum1.Zero, TrimmingOptions.Start,  true, Enum1.Two   },
        { "          ", Enum1.Two,  TrimmingOptions.Both,  false, Enum1.Two   },
        { "2         ", Enum1.Zero, TrimmingOptions.End,    true, Enum1.Two   },
        { "   3      ", Enum1.Zero, TrimmingOptions.Both,   true, Enum1.Three }
    };

    public static TheoryData<string, Guid?, bool, Guid?> GuidData => new()
    {
        { "   6f98ec4e-bf41-41d9-8220-12440d6158c7   ", null,                                              true, new Guid("6f98ec4e-bf41-41d9-8220-12440d6158c7") },
        { "   306a8d0f-a46c-479d-989f-a27544284e3d   ", null,                                              true, new Guid("306a8d0f-a46c-479d-989f-a27544284e3d") },
        { "   ff8fb86a-03c6-4aca-9a3e-9036e674079e",    new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),  true, new Guid("ff8fb86a-03c6-4aca-9a3e-9036e674079e") },
        { "   ff8fb86a-03c6-4aca-9a3e-9036e674079e",    new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),  true, new Guid("ff8fb86a-03c6-4aca-9a3e-9036e674079e") },
        { "                                          ", new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), false, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") },
        { "                                          ", default,                                          false, Guid.Empty },
        { "x                                         ", new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), false, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") }
    };

    public static TheoryData<string, DateOnly?, string?, bool, DateOnly?> DateData => new()
    {
        { "  2023-10-05  ", null,                   null,    true,  new DateOnly(2023, 10, 5) },
        { "05/10/2023",     null,                   "fr-FR", true,  new DateOnly(2023, 10, 5) },
        { "10/05/2023",     null,                   "en-US", true,  new DateOnly(2023, 10, 5) },
        { "2023-10-05",     null,                   null,    true,  new DateOnly(2023, 10, 5) },
        { "2023.10.05",     null,                   "de-DE", true,  new DateOnly(2023, 10, 5) },
        { "2023/10/05",     null,                   "ja-JP", true,  new DateOnly(2023, 10, 5) },
        { "          ",     null,                   null,    false, default(DateOnly) },
        { "          ",     new DateOnly(2022,1,1), null,    false, new DateOnly(2022,1,1) },
        { null!,            null,                   null,    false, default(DateOnly) },
        { "  20250916",     null,                   null,    true,  new DateOnly(2025, 9, 16) }
    };

    public static TheoryData<string, TimeOnly?, string?, bool, TimeOnly?> TimeData => new()
    {
        { "  18:30:45  ", null,                   null,    true,  new TimeOnly(18, 30, 45) },
        { "18:30:45",     null,                   null,    true,  new TimeOnly(18, 30, 45) },
        { "18.30.45",     null,                   "de-DE", true,  new TimeOnly(18, 30, 45) },
        { "18:30:45",     null,                   "fr-FR", true,  new TimeOnly(18, 30, 45) },
        { "18:30:45",     null,                   "ja-JP", true,  new TimeOnly(18, 30, 45) },
        { "          ",   null,                   null,    false, default(TimeOnly) },
        { "          ",   new TimeOnly(1,1,1),    null,    false, new TimeOnly(1,1,1) },
        { null!,          null,                   null,    false, default(TimeOnly) },
        { "183045",       null,                   null,    true,  new TimeOnly(18, 30, 45) }
    };

    public static TheoryData<string, DateTime?, string?, bool, DateTime?> DateTimeData => new()
    {
        { "  2023-10-05 18:30:45  ", null,                   null,    true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "05/10/2023 18:30:45",     null,                   "fr-FR", true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "10/05/2023 6:30:45 PM",   null,                   "en-US", true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "2023-10-05T18:30:45",     null,                   null,    true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "2023.10.05 18:30:45",     null,                   "de-DE", true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "2023/10/05 18:30:45",     null,                   "ja-JP", true,  new DateTime(2023, 10, 5, 18, 30, 45) },
        { "          ",              null,                   null,    false, default(DateTime) },
        { "          ",              new DateTime(2022,1,1), null,    false, new DateTime(2022,1,1) },
        { null!,                     null,                   null,    false, default(DateTime) },
        { "20231005183045",          null,                   null,    true,  new DateTime(2023, 10, 5, 18, 30, 45) }
    };

    public static TheoryData<string, DateTimeOffset?, string?, bool, DateTimeOffset?> DateTimeOffsetData => new()
    {
        { "  2023-10-05 18:30:45 +02:00  ", null,                                             null,    true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "05/10/2023 18:30:45 +02:00",     null,                                             "fr-FR", true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "10/05/2023 6:30:45 PM +02:00",   null,                                             "en-US", true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "2023-10-05T18:30:45+02:00",     null,                                              null,    true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "2023.10.05 18:30:45 +02:00",     null,                                             "de-DE", true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "2023/10/05 18:30:45 +02:00",     null,                                             "ja-JP", true,  new DateTimeOffset(2023, 10, 5, 18, 30, 45, TimeSpan.FromHours(2)) },
        { "          ",                     null,                                             null,    false, default(DateTimeOffset) },
        { "          ",                     new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero), null,    false, new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero) },
        { null!,                            null,                                             null,    false, default(DateTimeOffset) },
        { " 2023-10-05T18:30:45 +02:00 ",   null,                                             null,    true , new DateTimeOffset(2023,10,5,18,30,45,TimeSpan.FromHours(2)) }
    };

    public static TheoryData<string, TimeSpan?, string?, bool, TimeSpan?> TimeSpanData => new()
    {
        { "  12:30:45  ",        null,                   null,    true,  new TimeSpan(12, 30, 45) },
        { "1.12:30:45",          null,                   null,    true,  new TimeSpan(1, 12, 30, 45) },
        { "  1.12:30:45  ",      null,                   null,    true,  new TimeSpan(1, 12, 30, 45) },
        { "  1:30:45.1234567  ", null,                   null,    true,  new TimeSpan(0, 1, 30, 45, 123) + TimeSpan.FromTicks(4567) },
        { "12:30:45",            null,                   null,    true,  new TimeSpan(12, 30, 45) },
        { "          ",          null,                   null,    false, default(TimeSpan) },
        { "          ",          new TimeSpan(1,1,1),    null,    false, new TimeSpan(1,1,1) },
        { null!,                 null,                   null,    false, default(TimeSpan) },
        { "7.1:2:3",            null,                   null,    true,  new TimeSpan(7, 1, 2, 3) }
    };
}
