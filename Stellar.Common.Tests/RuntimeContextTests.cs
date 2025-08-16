namespace Stellar.Common.Tests;

public class RuntimeContextTests
{
    [Fact]
    public void Loads()
    {
        Assert.True(RuntimeContext.IsTesting);

        _ = RuntimeContext.Version;
    }
}
