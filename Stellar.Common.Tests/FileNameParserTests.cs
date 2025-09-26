using System.Reflection;
using System.Text;

namespace Stellar.Common.Tests;

public class FileNameParserTests
{
    private readonly string DataPath = @$"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Data";

    public static TheoryData<string, string, string, string> FakeFiles => new()
    {
        { "Hello(?<timestamp>.+).txt", "Hello20241231.txt", "{timestamp:yyyy-MM-dd}-Goodbye.htm", "2024-12-31-Goodbye.htm" },
        { "Hello(?<timestamp>.+).csv", "Hello20241231.csv.zip", "Hello{timestamp}.tsv", "Hello20241231000000.tsv" }, // gotcha...
        { "Hello(?<timestamp>.+).csv", "Hello20241231.csv.zip", "Hello{timestamp:yyyy-MM-dd}.tsv", "Hello2024-12-31.tsv" },
        { "(?<timestamp>.+).txt", "20241231.txt", "{timestamp:yyyy-MM-dd}.txt", "2024-12-31.txt" },
        { ".+statement(?<timestamp>.+).pdf$", "bank_statement202507251843.pdf", "bank_statement{timestamp:yyyy-MM-dd}.csv", "bank_statement2025-07-25.csv" },
        { "Base(?<base>[^0-9]+)(?<timestamp>.+).xlsx", "Baseball20241231.xlsx", "Basket{base}-{timestamp:yyyy-MM}.csv", "Basketball-2024-12.csv" },
        { "(?<base>[^0-9]+)ball(?<timestamp>.+).xlsx", "Baseball2025-07-26.xlsx", "{base}ketball-{timestamp:yyyyMMdd}.csv", "Baseketball-20250726.csv" }
    };

    [Theory]
    [MemberData(nameof(FakeFiles))]
    public void ParsesFakeFiles(
        string pattern,
        string filename,
        string template,
        string expected)
    {
        var result = FileNameParser.TryParse(filename, pattern, template, out var output);
        
        Assert.True(result);
        Assert.NotNull(output);

        Assert.Equal(expected, output.FileName);
        Assert.NotNull(output.Timestamp);
        Assert.False(output.Exists);
        Assert.True(output.IsReadOnly);
        Assert.Equal(0, output.Length);
    }

    public static TheoryData<string, string, string, string, long> ExistingFiles => new()
    {
        { @"(?<base>.+)(?<number>\d\d).csv", "file01.csv", "{base}-{number}.tsv", "file-01.tsv", 442L },
        { @"persons.csv", "persons.csv", "{base}-{timestamp:yyyy-MM-dd}.tsv", $"persons-{DateOnly.FromDateTime(DateTime.Now):yyyy-MM-dd}.tsv", 100162L },
    };

    [Theory]
    [MemberData(nameof(ExistingFiles))]
    public void ParsesExistingFiles(
        string pattern,
        string filename,
        string template,
        string expected,
        long length)
    {
        var filePath = Path.Combine(DataPath, filename);

        var input = new FileInfoEx(filePath);

        Assert.NotNull(input);

        Assert.True(input.Exists);
        Assert.Equal(length, input.Length);

        var result = FileNameParser.TryParse(input, pattern, template, out var output);
        
        Assert.True(result);
        Assert.NotNull(output);

        Assert.Equal(expected, output.FileName);
        Assert.NotNull(output.Timestamp);
        Assert.False(output.Exists);
        Assert.True(output.IsReadOnly);
        Assert.Equal(0, output.Length);

        var isoEncoding = Encoding.GetEncoding("iso-8859-1");

        Assert.Equal("Western European (ISO)", isoEncoding.EncodingName);

        var reader = new StreamReader(filePath);

        var utfEncoding = reader.CurrentEncoding;

        Assert.Equal(65001, utfEncoding.CodePage);

        reader = new StreamReader(filePath, isoEncoding);

        Assert.Equal(28591, reader.CurrentEncoding.CodePage);
    }

    [Theory]
    [InlineData("ISO-8859-1", "S+âGÇª+é-½r+âGÇP+é-½+âGÇP+é-ün")]
    public void SetsAndGetsFileEncoding(
        string encodingName,
        string expected)
    {
        var filePath = Path.Combine(DataPath, "address.tsv");

        var reader = new StreamReader(filePath, ValueConverter.ParseEncoding(encodingName));

        var i = 0;

        while (i++ < 80)
        {
            reader.ReadLine();
        }

        var line = reader.ReadLine();
        var values = line?.Split('\t');

        Assert.Equal(expected, values![2]);

        Assert.Equal(Encoding.Default, ValueConverter.ParseEncoding("bogus"));
    }

    public static TheoryData<string, string> NoMatchFiles => new()
    {
        { @"file(?<number>\d\d).csv", "bogus.csv" },
    };

    [Theory]
    [MemberData(nameof(NoMatchFiles))]
    public void ParsesNomatchFiles(
        string pattern,
        string filename)
    {
        var result = FileNameParser.TryParse(filename, pattern, "template", out var output);
        
        Assert.False(result);
    }

    [Theory]
    [InlineData("bogus.txt", "(?<base>bogus).txt", "{base} {created:yyyy-MM-dd} {modified:HH:mm}.csv")]
    public void ParsesFileInfoProperties(string name, string pattern, string template) {
        
        File.CreateText(name).WriteLine("Hello, World.");

        var info = new FileInfo(name);
        var created = info.CreationTime;
        var modified = info.LastWriteTime;
        var expectedBaseFileName = $"{Path.GetFileNameWithoutExtension(name)} {created:yyyy-MM-dd} {modified:HH:mm}";

        var result = FileNameParser.TryParse(info.FullName, pattern, template, out var output);

        Assert.True(result);
        Assert.NotNull(output);

        Assert.Equal(expectedBaseFileName + output.Extension, output.FileName);
        Assert.Equal(expectedBaseFileName, output.BaseFileName);
        Assert.Equal(info.DirectoryName, output.Path);
    }

    [Theory]
    [InlineData("lorem.txt", "(?<base>lorem).txt", "{filename} {createdutc:yyyy-MM-dd} {modifiedutc:HH:mm}.csv")]
    public void ParsesFileInfoUtcProperties(string name, string pattern, string template) {
        
        File.CreateText(name).WriteLine("Hello, World.");

        var info = new FileInfo(name);

        var createdUtc = info.CreationTimeUtc;
        var modifiedUtc = info.LastWriteTimeUtc;
        
        var expectedBaseFileName = $"{name} {createdUtc:yyyy-MM-dd} {modifiedUtc:HH:mm}";

        var result = FileNameParser.TryParse(info.FullName, pattern, template, out var output);

        Assert.True(result);
        Assert.NotNull(output);
        Assert.Equal(expectedBaseFileName + output.Extension, output.FileName);
        Assert.Equal(expectedBaseFileName, output.BaseFileName);
        Assert.Equal(info.DirectoryName, output.Path);
    }
}
