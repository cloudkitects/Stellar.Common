namespace Stellar.Common.Tests;

public class RuntimeContextTests
{
    [Fact]
    public void Loads()
    {
        Assert.True(RuntimeContext.IsTesting);
        
        Assert.NotEmpty(RuntimeContext.ExecutingAssembly);
        Assert.NotEmpty(RuntimeContext.EntryAssembly);
        Assert.NotEmpty(RuntimeContext.Version);
        Assert.Equal(System.Diagnostics.Debugger.IsAttached, RuntimeContext.IsDebugging);
    }
}
