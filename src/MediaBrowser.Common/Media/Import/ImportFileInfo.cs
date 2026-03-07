namespace MediaBrowser.Media.Import;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public record ImportFileInfo
{
    public static ImportFileInfo? Create(MediaConfig mediaConfig, string path)
    {
        var name = Path.GetFileName(path);
        if (name.StartsWith('.')
            || !name.Contains('.')
            || !mediaConfig.ImportExtensions.TryGetValue(Path.GetExtension(name)[1..].ToLowerInvariant(),
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
            Url = $"/api/import/file/{HttpUtility.UrlPathEncode(name)}"
        };
    }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("mime")]
    public required string Mime { get; init; }

    [JsonPropertyName("size")]
    public required long? Size { get; init; }

    [JsonPropertyName("ctimeMs")]
    public required long CtimeMs { get; init; }

    [JsonPropertyName("mtimeMs")]
    public required long MtimeMs { get; init; }

    [JsonPropertyName("createdOn")]
    public required DateTime? CreatedOn { get; init; }

    [JsonPropertyName("updatedOn")]
    public required DateTime? UpdatedOn { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }
}