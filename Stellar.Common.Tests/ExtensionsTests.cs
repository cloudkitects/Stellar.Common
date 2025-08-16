namespace Stellar.Common.Tests;

public class ExtensionsTests
{
    #region char
    public static readonly TheoryData<char, bool> CharTestData = new() {
        { '0', true },
        { '1', true },
        { '2', true },
        { '3', true },
        { '3', true },
        { '4', true },
        { '5', true },
        { '6', true },
        { '7', true },
        { '8', true },
        { '9', true },
        { '+', true },
        { '-', true },
        { 'A', false },
        { 'B', false },
        { 'x', false },
        { 'y', false },
        { '!', false }
    };

    [Theory]
    [MemberData(nameof(CharTestData))]
    public void DetectsNumbersOrSigns(char c, bool e)
    {
        Assert.Equal(e, c.IsNumberOrSign());
    }
    #endregion

    #region numeric
    public static readonly TheoryData<int, int, int, bool> IntTestData = new()
    {
        { 1, 2, 3, false },
        { 2, 2, 3, true },
        { 3, 2, 3, true },
        { 4, 2, 3, false },
        { 0, 0, 0, true },
        { 2, 1, 0, false },
        { 1, 1, 0, true }
    };

    [Theory]
    [MemberData(nameof(IntTestData))]
    public void BetweensInt(int a, int b, int c, bool e)
    {
        Assert.Equal(e, a.Between(b, c));
    }

    public static readonly TheoryData<double, double, double, bool> DoubleTestData = new()
    {
        { 1.99999, 2.0, 3d, false },
        { 2.00001, 2.0, 3d, true },
        { 3.0, 2.0, 3d, true },
        { 3.00001, 3d, 2.0, false },
        { 0d, 0.0, 0d, true },
        { 20d, 1.0, 0d, false },
        { 1.000, 1.0, 0d, true }
    };

    [Theory]
    [MemberData(nameof(DoubleTestData))]
    public void BetweensDouble(double a, double b, double c, bool e)
    {
        Assert.Equal(e, a.Between(b, c));
    }

    [Theory]
    [MemberData(nameof(DoubleTestData))]
    public void ClampsDouble(double a, double b, double c, bool _)
    {
        var r = a.Clamp(b, c);

        Assert.True(b <= r || r <= c);
    }

    public class TestObject(int a, bool b, string c)
    {
        public int A { get; set; } = a;
        public bool B { get; set; } = b;
        public string C { get; set; } = c;

        public double D = 3.14;

        public double E() { return A * D; }
    }

    public static TheoryData<TestObject> TestObjects =
    [
        new TestObject(1, false, "no" ),
        new TestObject(2, true, "yes" ),
    ];

    [Theory]
    [MemberData(nameof(TestObjects))]
    public void ConvertsToDynamicDictionary(TestObject o)
    {
        dynamic d = o.ToDynamicDictionary();

        Assert.True(TypeCache.TryGet(typeof(TestObject), out _));

        Assert.Equal(o.A, d.A);
        Assert.Equal(o.B, d["b"]);
        Assert.Equal(o.C, d["C"]);
        Assert.Equal(o.D, d.d);

        var e = new Dictionary<string, object>()
        {
            { "a", new DateOnly(2025, 08, 15) },
            { "b", 2 },
            { "c", "tomorrow" },
        };

        dynamic f = e.ToDynamicDictionary();

        Assert.Equal(e["a"], f.A);
    }

    [Fact]
    public void RecognizesAnonymousType()
    {
        var o = new { A = 1, B = "test" };
        int? p = null;

        Assert.True(o.IsAnonymousType());
        Assert.False(p!.IsAnonymousType());
    }


    [Fact]
    public void ExtendsString()
    {
        var j = @"Joe ""eat-at"" Joe";
        var k = "Kay \"kay\" Bier";
        var l = "Lia 'son' Swan";

        var x = new { A = 1, B = "test" }.ToString();
        var s = "    ";
        var t = s.Trim();
        var m = "-55";
        var n = "44";
        var o = ".14";
        var p = "  -1.14";
        var q = " -";

        Assert.Equal("\"Joe \"\"eat-at\"\" Joe\"", j.Qualify());
        Assert.Equal("\"Kay \\\"kay\\\" Bier\"", k.Qualify(e: '\\'));
        Assert.Equal("'Lia ''son'' Swan'", l.Qualify('\'', '\''));

        Assert.Equal("0636148e-631c-a9df-14eb-ab2083e1916c", x!.Hash().ToString());

        Assert.True(s.IsNullOrWhiteSpace());
        Assert.True(t.IsNullOrEmpty());
        Assert.Null(t.NullIfEmpty());
        Assert.NotNull(j.NullIfEmpty());

        Assert.True(m.IsNumericAt(0));
        Assert.True(n.IsNumericAt(0));
        Assert.True(o.IsNumericAt(0));
        Assert.True(p.IsNumericAt(2));

        Assert.False(s.IsNumericAt(-1));
        Assert.False(s.IsNumericAt(5));
        Assert.False(q.IsNumericAt(1));

        Assert.True('a'.IsIdentifier());
        Assert.True('Z'.IsIdentifier());
        Assert.True('_'.IsIdentifier());
    }

    [Fact]
    public void ExtendsDateTime()
    {
        var date = new DateTime(2023, 10, 15);

        Assert.Equal(364, date.YearTotalDays());
        Assert.Equal(2023287, date.ToJulianDate());
    }
    #endregion
}
