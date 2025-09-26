namespace Stellar.Common;

public class FileInfoEx(string filename)
{
    private readonly FileInfo fileInfo = new(filename);

    public string Path => fileInfo.DirectoryName ?? string.Empty; // defaults to the current directory
    public string FileName => fileInfo.Name;
    public string BaseFileName => fileInfo.Name[..^fileInfo.Extension.Length];
    public string Extension => fileInfo.Extension;
    public DateTime Created => fileInfo.CreationTime;
    public DateTime CreatedUtc => fileInfo.CreationTimeUtc;
    public DateTime Modified => fileInfo.LastWriteTime;
    public DateTime ModifiedUtc => fileInfo.LastWriteTimeUtc;
    public bool Exists => fileInfo.Exists;
    public bool IsReadOnly => fileInfo.IsReadOnly;
    public long Length { get { try { return fileInfo.Length; } catch { return 0; } } }
    public DateTime? Timestamp { get; internal set; }
}