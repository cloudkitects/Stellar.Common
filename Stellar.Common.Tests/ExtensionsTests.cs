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
    #endregion
}
