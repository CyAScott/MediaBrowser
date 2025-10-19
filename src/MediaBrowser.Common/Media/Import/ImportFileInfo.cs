using System.Web;

namespace MediaBrowser.Media.Import;

public class ImportFileInfo
{
    public static ImportFileInfo? Create(MediaConfig mediaConfig, string path)
    {
        var name = Path.GetFileName(path);
        if (name.StartsWith(".")
            || !name.Contains('.')
            || !mediaConfig.ImportExtensions.TryGetValue(Path.GetExtension(name).Substring(1).ToLowerInvariant(), 
                out var ext))
        {
            return null;
        }

        var fileInfo = new FileInfo(path);
        
        return new()
        {
            CreatedOn = fileInfo.CreationTime,
            CtimeMs = Convert.ToInt64((fileInfo.CreationTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            Mime = ext.Mime,
            MtimeMs = Convert.ToInt64((fileInfo.LastWriteTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            Name = name,
            Size = fileInfo.Length,
            UpdatedOn = fileInfo.LastWriteTime,
            Url = $"/api/import/file/{HttpUtility.UrlPathEncode(name)}",
        };
    }

    public required string Name { get; init; }

    public required string Mime { get; init; }

    public required long? Size { get; init; }

    public required long CtimeMs { get; init; }

    public required long MtimeMs { get; init; }

    public required DateTime? CreatedOn { get; init; }

    public required DateTime? UpdatedOn { get; init; }

    public required string Url { get; init; }
}