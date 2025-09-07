namespace Stellar.Common.Tests;

public class EnumHelperTests
{
    public enum TestEnum1
    {
        Value1,
        Value2,
        Value3
    }

    [Fact]
    public void GetsEnumInfo1()
    {
        var type = TestEnum1.Value3.GetType();

        var info = EnumHelper.GetEnumInfo(type);

        Assert.NotNull(info);
        Assert.Equal(3, info.Values.Length);

        Assert.Equal("Value1", info.Names[0]);
        Assert.Equal("Value2", info.Names[1]);
        Assert.Equal("Value3", info.Names[2]);

        Assert.Equal(0L, info.Values[0]);
        Assert.Equal(1L, info.Values[1]);
        Assert.Equal(2L, info.Values[2]);
        Assert.Equal(0L, info.AllFlags);

        Assert.False(info.IsFlags);
    }

    public enum SByteEnum : sbyte
    {
        Value0 = 32,
        Value1 = -1,
        Value2 = 42,
        Value3 = 0
    }

    [Fact]
    public void GetsEnumInfo2()
    {
        var type = SByteEnum.Value2.GetType();

        var info = EnumHelper.GetEnumInfo(type);

        Assert.NotNull(info);
        Assert.Equal(4, info.Values.Length);

        Assert.Equal("Value1", info.Names[0]);
        Assert.Equal("Value3", info.Names[1]);
        Assert.Equal("Value0", info.Names[2]);
        Assert.Equal("Value2", info.Names[3]);

        Assert.Equal(-1L, info.Values[0]);
        Assert.Equal(0L, info.Values[1]);
        Assert.Equal(32L, info.Values[2]);
        Assert.Equal(42L, info.Values[3]);

        Assert.Equal(0L, info.AllFlags);

        Assert.False(info.IsFlags);

        Assert.Equal("Value1", EnumHelper.GetName(type, -1));
        Assert.Equal("Value2", EnumHelper.GetName(SByteEnum.Value2));

        Assert.Null(EnumHelper.GetName(type, 1968));
        Assert.False(EnumHelper.IsDefined<SByteEnum>(1968));
    }

    [Flags]
    public enum ByteEnum : byte
    {
        B2 = 2,
        B4 = 4,
        B8 = 8,
        BT = 16,
        BA = B2 | B4 | B8 | BT
    }

    [Fact]
    public void GetsNames()
    {
        var names = EnumHelper.GetNames<ByteEnum>();

        var info = EnumHelper.GetEnumInfo(typeof(ByteEnum));

        Assert.Equal(5, names.Length);
        Assert.Equal("B2", names[0]);
        Assert.Equal("B4", names[1]);
        Assert.Equal("B8", names[2]);
        Assert.Equal("BT", names[3]);
        Assert.Equal("BA", names[4]);

        Assert.True(EnumHelper.IsDefined<ByteEnum>(16));
        Assert.True(EnumHelper.IsDefined<ByteEnum>(30));
        Assert.False(EnumHelper.IsDefined<ByteEnum>(32));

        Assert.Equal(30L, info.AllFlags);
        Assert.Equal((byte)30, (byte)ByteEnum.BA);
    }

    [Fact]
    public void GetsValues()
    {
        var values1 = EnumHelper.GetValues<ByteEnum>();

        Assert.Equal(5, values1.Length);

        Assert.Equal(ByteEnum.B2, values1[0]);
        Assert.Equal(ByteEnum.B4, values1[1]);
        Assert.Equal(ByteEnum.B8, values1[2]);
        Assert.Equal(ByteEnum.BT, values1[3]);
        Assert.Equal(ByteEnum.BA, values1[4]);

        var values2 = EnumHelper.GetValues(typeof(ByteEnum));

        Assert.Equal(5, values2.Length);

        Assert.Equal(ByteEnum.B2, values2.GetValue(0));
        Assert.Equal(ByteEnum.B4, values2.GetValue(1));
        Assert.Equal(ByteEnum.B8, values2.GetValue(2));
        Assert.Equal(ByteEnum.BT, values2.GetValue(3));
        Assert.Equal(ByteEnum.BA, values2.GetValue(4));
    }

    [Fact]
    public void ParsesOrThrows()
    {
        Assert.Equal(ByteEnum.BA, EnumHelper.Parse<ByteEnum>("ba", true));
        Assert.Throws<ArgumentException>(() => EnumHelper.Parse<ByteEnum>("ba", false));

        Assert.Equal(ByteEnum.BT, EnumHelper.Parse(typeof(ByteEnum), "bT", true));
        Assert.Throws<ArgumentException>(() => EnumHelper.Parse(typeof(ByteEnum), "bb8", false));
    }

    [Flags]
    public enum LongEnum : long
    {
        L1 = -2,
        L2 = -5
    }

    [Fact]
    public void TriesToParse()
    {
        Assert.True(EnumHelper.TryParse(typeof(ByteEnum), "bT", true, out object? obj));
        Assert.Equal(ByteEnum.BT, obj);

        Assert.False(EnumHelper.TryParse(typeof(ByteEnum), "bT", false, out obj));
        Assert.Null(obj);

        Assert.False(EnumHelper.TryParse(typeof(ByteEnum), "bb8", false, out obj));
        Assert.Null(obj);

        Assert.False(EnumHelper.TryParse(typeof(ByteEnum), null!, false, out obj));
        Assert.Null(obj);

        Assert.True(EnumHelper.TryParse(typeof(SByteEnum), "-1", true, out obj));
        Assert.Equal(SByteEnum.Value1, obj);

        Assert.False(EnumHelper.TryParse(typeof(SByteEnum), "-2", true, out obj));
        Assert.Null(obj);

        Assert.False(EnumHelper.TryParse(typeof(SByteEnum), "-", true, out obj));
        Assert.Null(obj);

        Assert.True(EnumHelper.TryParse(typeof(LongEnum), "-1", true, out obj));
        Assert.Equal(LongEnum.L1 | LongEnum.L2, obj);

        Assert.False(EnumHelper.TryParse(typeof(SByteEnum), "bogus", true, out obj));
        Assert.Null(obj);

        Assert.True(EnumHelper.TryParse(typeof(SByteEnum), "Value2", true, out obj));
        Assert.Equal(SByteEnum.Value2, obj);

        Assert.False(EnumHelper.CheckEnumerationValue(obj!, false, false, "Value2", null!));
        Assert.True(EnumHelper.CheckEnumerationValue(SByteEnum.Value0, false, false, "Value2", 32, 42));
        Assert.False(EnumHelper.CheckEnumerationValue(SByteEnum.Value1, false, false, "Value2", 32, 42));
        Assert.False(EnumHelper.CheckEnumerationValue(null!, false, false, "Value2", 32, 42));
        Assert.Throws<ArgumentNullException>(() => EnumHelper.CheckEnumerationValue(null!, false, true, "Value2", 32, 42));
    }

    [Fact]
    public void ChecksEnumerationValues()
    {
        Assert.False(EnumHelper.CheckEnumerationValue(SByteEnum.Value2, false, false, "myprop", null!));
        Assert.True(EnumHelper.CheckEnumerationValue(SByteEnum.Value0, false, false, "myfield", 32, 42));
        Assert.False(EnumHelper.CheckEnumerationValue(SByteEnum.Value1, false, false, "myvalue", 32, 42));
        Assert.False(EnumHelper.CheckEnumerationValue(null!, false, false, "x", 32, 42));
        
        Assert.False(EnumHelper.CheckEnumerationValue(ByteEnum.BT, true, false, "byte", 2, 4, 8));
        Assert.True(EnumHelper.CheckEnumerationValue(-1, true, false, "gauge", -2, -5));
        
        Assert.Throws<ArgumentException>(() => EnumHelper.CheckEnumerationValue(LongEnum.L1, false, true, "myprop", null!));
        Assert.Throws<ArgumentNullException>(() => EnumHelper.CheckEnumerationValue(null!, false, true, "reading", 32, 42));
    }

    [Fact]
    public void ChecksEnumerationValuesByMinMax()
    {
        Assert.True(EnumHelper.CheckEnumerationValueByMinMax(SByteEnum.Value2, false, "myprop", -1, 42));
        Assert.True(EnumHelper.CheckEnumerationValueByMinMax(SByteEnum.Value2, false, "  ", -1, 42));
        Assert.False(EnumHelper.CheckEnumerationValueByMinMax(null!, false, "  ", -1, 42));
        Assert.True(EnumHelper.CheckEnumerationValueByMinMax(5, false, "  ", -1, 42));
        Assert.False(EnumHelper.CheckEnumerationValueByMinMax(43, false, "hello", -1, 42));
        Assert.False(EnumHelper.CheckEnumerationValueByMinMax(-2, false, "hello", -1, 42));
        
        Assert.Throws<ArgumentNullException>(() => EnumHelper.CheckEnumerationValueByMinMax(null!, throwOnError: true, "  ", -1, 42));
        Assert.Throws<ArgumentException>(() => EnumHelper.CheckEnumerationValueByMinMax(int.MinValue, throwOnError: true, "bogus", -1, 42));
    }

    [Fact]
    public void ChecksEnumerationValuesByMask()
    {
        Assert.True(EnumHelper.CheckEnumerationValueByMask(SByteEnum.Value2, false, "myprop", -1));
        Assert.True(EnumHelper.CheckEnumerationValueByMask(SByteEnum.Value2, false, "", -1));
        Assert.True(EnumHelper.CheckEnumerationValueByMask(-1, false, "", -1));
        Assert.False(EnumHelper.CheckEnumerationValueByMask(-1, false, "", 0));
        Assert.False(EnumHelper.CheckEnumerationValueByMask(null!, false, "", -1));
        
        Assert.Throws<ArgumentNullException>(() => EnumHelper.CheckEnumerationValueByMask(null!, true, "nada", -1));
        Assert.Throws<ArgumentException>(() => EnumHelper.CheckEnumerationValueByMask(1, true, "todo", 0));
    }


    [Fact]
    public void GetsEnumInfoThrows()
    {
        var type1 = (Type)null!;
        var type2 = "NotAnEnum".GetType();

        Assert.Throws<ArgumentNullException>(() => EnumHelper.GetEnumInfo(type1));
        Assert.Throws<ArgumentException>(() => EnumHelper.GetEnumInfo(type2));
    }
}
