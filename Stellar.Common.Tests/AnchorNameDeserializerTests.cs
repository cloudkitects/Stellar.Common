using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Stellar.Common.Tests;

public class Options
{
    public List<FileSpec> Sources { get; set; } = [];
    public List<FileSpec> Targets { get; set; } = [];
    public List<Run> Runs { get; set; } = [];

    public class FileSpec : IAnchoredObject
    {
        public string? Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string Pattern { get; set; } = ".*";
        [YamlMember(Alias = "encoding")]
        public string EncodingName { get; set; } = Encoding.Default.EncodingName;
    }

    public class Run
    {
        public required FileSpec Source { get; set; }
        public required FileSpec Target { get; set; }
    }
}

public class AnchorNameDeserializerTests
{
    public static string data = @"
sources:
- &source1
  path: C:\Dispatch\Source1\Input
  pattern: address-typed(?<timestamp>\d{4}-\d{2}-\d{2})\.csv
  encoding: ISO-8859-1

targets:
- &target1
  path: C:\Dispatch\Source1\Output
  template: ""{base}.tsv""
  delimiter: ""\t""
  headers: [ Address Id, Line1, City, Zip ]
  key: [ Address Id ]
  hash: true

runs:
- source: *source1
  target: *target1";

    [Fact]
    public void DeserializeOptionsWithAnchors()
    {
        var valueDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .BuildValueDeserializer();

        var deserializer = Deserializer.FromValueDeserializer(new AnchorNameDeserializer(valueDeserializer));

        var options = deserializer.Deserialize<Options>(data);
        
        Assert.NotNull(options);
        Assert.Single(options.Runs);
        
        var source = options.Runs[0].Source;
        var target = options.Runs[0].Target;
        
        Assert.Equal("C:\\Dispatch\\Source1\\Input", source.Path);
        Assert.Equal("address-typed(?<timestamp>\\d{4}-\\d{2}-\\d{2})\\.csv", source.Pattern);
        Assert.Equal("ISO-8859-1", source.EncodingName);
        Assert.Equal("source1", source.Name);
        Assert.Equal("C:\\Dispatch\\Source1\\Output", target.Path);
        Assert.Equal("target1", target.Name);
        
    }
}
