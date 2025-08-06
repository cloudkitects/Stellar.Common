namespace Stellar.Common.Tests;

public class BucketTests
{
    [Fact]
    public void BucketWorks()
    {
        var bucket = new Bucket<string>([ "apples", "oranges", "bananas" ]);

        bucket.Add("apples");

        Assert.True(bucket["apples"]);
        Assert.False(bucket["oranges"]);
        Assert.False(bucket["bananas"]);
        
        Assert.False(bucket.IsFull);

        bucket.Add("oranges");
        bucket.Add("bananas");

        Assert.True(bucket.IsFull);
    }
}
