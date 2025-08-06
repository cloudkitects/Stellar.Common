using System.Text;
using System.Text.RegularExpressions;

namespace Stellar.Common;

public static partial class FileNameParser
{
    [GeneratedRegex("\\{([a-z]+):*([^}]*)\\}")]
    private static partial Regex PlaceholderRegex();

    public static bool TryParse(string filename, string pattern, string template, out FileInfoEx? result)
    {
        return TryParse(new FileInfoEx(filename), pattern, template, out result);
    }

    public static bool TryParse(FileInfoEx fileInfo, string pattern, string template, out FileInfoEx? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        var match = Regex.Match(fileInfo.FileName, pattern);

        if (!match.Success)
        {
            result = fileInfo;

            return false;
        }

        fileInfo.Timestamp = ValueConverter.TryParseDateTime(match.Groups["timestamp"].Value, out var timestamp) ? timestamp : DateTime.Now;

        var builder = new StringBuilder(template);

        foreach (var placeholder in PlaceholderRegex().Matches(template).Cast<Match>())
        {
            var name = placeholder.Groups[1].Value;
            var format = placeholder.Groups[2].Length > 0
                ? placeholder.Groups[2].Value
                : name == "timestamp" ? "yyyyMMddHHmmss" : "G";

            var undefined = !match.Groups.ContainsKey(name);
            var groupValue = match.Groups[name].Value;

            var value = string.Format($"{{0:{format}}}", name switch
            {
                "base" when undefined => fileInfo.BaseFileName,
                "filename" when undefined => fileInfo.FileName,
                "created" => fileInfo.CreationTime,
                "createdutc" => fileInfo.CreationTimeUtc,
                "modified" => fileInfo.LastWriteTime,
                "modifiedutc" => fileInfo.LastWriteTimeUtc,
                "timestamp" => fileInfo.Timestamp,
                _ => groupValue
            });

            builder.Replace(placeholder.Value, value);
        }

        result = new FileInfoEx(builder.ToString()) { Timestamp = fileInfo.Timestamp };

        return !result.Equals(fileInfo);
    }

}
