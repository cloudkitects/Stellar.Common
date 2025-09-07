namespace Stellar.Common.Tests;

public partial class ValueConverterTests
{
    /// <remarks>
    /// Parenthesis not allowed by default, but under user control.
    /// Thousands separator allowed by default.
    /// <see cref="ValueConverter.IntegerNumberStyles"/>.
    /// </remarks>
    public static TheoryData<string, int?, string?, bool, int> IntegerData => new()
    {
        { "67",         -1, null,    true,    67 },
        { "-67",        -1, null,    true,   -67 },
        { "(67)",       -1, null,    false,   -1 },
        { " 67 ",       -1, null,    true,    67 },
        { " 67 ",       -1, null,    true,    67 },
        { "    ",     null, null,    false,    0 },
        { null!,      null, null,    false,    0 },
        { " 67.500 ",   -2, "de-DE", true, 67500 },
        { " 67,500 ",   -2, null,    true, 67500 },
        { " 67.50  ", null, "de-DE", true,  6750 }
    };
}
