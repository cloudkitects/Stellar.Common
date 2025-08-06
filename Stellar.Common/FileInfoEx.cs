using System.Text;

namespace Stellar.Common;

public class FileInfoEx(string filename)
{
    private readonly FileInfo fileInfo = new(filename);

    public string Path => fileInfo.DirectoryName ?? string.Empty; // defaults to the current directory
    public string FileName => fileInfo.Name;
    public string BaseFileName => fileInfo.Name[..^fileInfo.Extension.Length];
    public string Extension => fileInfo.Extension;
    public DateTime CreationTime => fileInfo.CreationTime;
    public DateTime CreationTimeUtc => fileInfo.CreationTimeUtc;
    public DateTime LastWriteTime => fileInfo.LastWriteTime;
    public DateTime LastWriteTimeUtc => fileInfo.LastWriteTimeUtc;
    public bool Exists => fileInfo.Exists;
    public bool IsReadOnly => fileInfo.IsReadOnly;
    public long Length { get { try { return fileInfo.Length; } catch { return 0; } } }
    public DateTime? Timestamp { get; internal set; }
}