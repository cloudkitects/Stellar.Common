namespace Stellar.Common.Tests;

public class CollectionExtensionsTests
{
	#region array
	[Fact]
	public void ArrayInsertThrows()
	{
		var array = new[] { 1, 2, 4, 5 };
		
		Assert.Throws<ArgumentOutOfRangeException>(() => array.InsertAt(-1, 3));
		Assert.Throws<ArgumentOutOfRangeException>(() => array.InsertAt( 5, 3));
    }

	[Fact]
	public void InsertsIntoArray()
	{
		var a = new int?[] { 1, 2, 4, 5, null, null };
		var b = new int?[] { 1, 2, 3, 4, 5, null, null };
		
		var c = a.InsertAt(2, 3);

		Assert.True(b.SequenceEqual(c));
    }

	[Fact]
	public void InsertsIntoArrayAndTrims()
	{
		var a = new int?[] { 1, 2, 4, null, null };
		var b = new int?[] { 1, 2, 3, 4 };
		
		var c = a.InsertAt(2, 3, true);

		Assert.True(b.SequenceEqual(c));
    }

	[Fact]
	public void ArrayRemoveAtThrows()
	{
		var array = new[] { 1, 2, 4, 5 };
		
		Assert.Throws<ArgumentOutOfRangeException>(() => array.RemoveAt(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => array.RemoveAt( 4));
    }


	[Fact]
	public void RemovesFromArray()
	{
		var a = new int?[] { 1, 2, 3, 4, null, null };
		var b = new int?[] { 1, 3, 4, null, null };
		
        var c = a.Remove(2, false);

		Assert.True(b.SequenceEqual(c));
    }


	[Fact]
	public void RemovesFromArrayAndTrims()
	{
		var a = new int?[] { 1, 2, 3, 4, null, null };
		var b = new int?[] { 1, 3, 4 };
		var c = new int?[] { 1, 2, 4 };
		
        var d = a.Remove(2, true);
        var e = a.Remove(5, true);
        var f = a.Remove(3, true);

		Assert.True(b.SequenceEqual(d));
		Assert.True(e.SequenceEqual(a));
		Assert.True(f.SequenceEqual(c));
    }

	[Fact]
	public void IsNullOrEmpty()
	{
		int[] a = [];
		int[] b = null!;
		
		Assert.True(a.IsNullOrEmpty());
		Assert.True(b.IsNullOrEmpty());
    }

    #endregion
}
