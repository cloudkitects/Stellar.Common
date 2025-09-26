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

	[Fact]
	public void TriesToGetValueAt()
	{
		var a = new int?[] { 0, 1, 2, 3, 4 };

		Assert.False(a.TryGetValueAt(-1, out var _));
        Assert.False(a.TryGetValueAt(5, out _));
		Assert.True(a.TryGetValueAt(2, out var value));
		Assert.Equal(2, value);
    }
    #endregion

    #region dictionary
	[Fact]
	public void VariousTests()
	{
		var dict = new Dictionary<string, object?>()
		{
			{ "one", 1 },
			{ "two", 2 },
			{ "three", 3 },
			{ "four", 4 },
			{ "five", 5 }
		};

		var slice = dict.Slice([ "one", "three", "five" ]);

		Assert.Equal(3, slice.Count);
		Assert.Equal(1, slice["one"]);
		Assert.Equal(3, slice["three"]);
		Assert.Equal(5, slice["five"]);

		var splice = dict.Splice([ "one", "three", "five" ]);

		Assert.Equal(2, splice.Count);
		Assert.Equal(2, splice["two"]);
		Assert.Equal(4, splice["four"]);

		var merge = splice.Merge(slice);

		Assert.Equal(5, merge.Count);
		Assert.Equal(1, merge["one"]);
		Assert.Equal(2, merge["two"]);
		Assert.Equal(3, merge["three"]);
		Assert.Equal(4, merge["four"]);
		Assert.Equal(5, merge["five"]);

		var hash = merge.Hash();

		Assert.Equal(new Guid("335e2b2c-e82a-316f-735c-357e412f2c5e"), hash);

		merge.AddOrUpdate("five", -5);
		merge.AddOrUpdate("six", 6);
		
		Assert.Equal(-5, merge["five"]);
		Assert.Equal(6, merge["six"]);
    }
    #endregion
}
