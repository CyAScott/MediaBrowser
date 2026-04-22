namespace MediaBrowser.Media;

public class MediaConfig(IConfiguration configuration)
{
    public string CastDirectory { get; } = configuration["media:castDirectory"]!;
    public string DeletedDirectory { get; } = configuration["media:deletedDirectory"]!;
    public string DirectorsDirectory { get; } = configuration["media:directorsDirectory"]!;
    public string GenresDirectory { get; } = configuration["media:genresDirectory"]!;
    public string? ImportDirectory { get; } = configuration["media:importDirectory"];

    /// <summary>
    /// A dictionary of file extensions and their associated MIME types and order for import.
    /// </summary>
    public IReadOnlyDictionary<string, FileExtensionInfo> ImportExtensions { get; } =
        configuration.GetSection("media:importExtensions").Get<Dictionary<string, FileExtensionInfo>>()!;

    public bool TryToGetExtensionFromMime(string? mime, out string ext)
    {
        ext = ImportExtensions.Values
            .OrderBy(it => it.Order)
            .FirstOrDefault(it => it.Mime.Equals(mime, StringComparison.OrdinalIgnoreCase))?.Ext!;
        return ext != null!;
    }

    public string GetExtensionFromMime(string mime)
    {
        Guard.Against.Default(TryToGetExtensionFromMime(mime, out var ext));
        return ext;
    }

    /// <summary>
    /// Creates a file location for a media file based on its MD5 hash and optional parent ID.
    /// If the media has a parent ID, it appends it to the file name to allow for multiple files with the same hash (e.g., different chapters).
    /// The extension is determined from the media's MIME type, or can be overridden with the optional extension parameter.
    /// </summary>
    public string MediaFileLocation(MediaEntity media, string? extension = null) =>
        Path.Combine(MediaDirectory, $"{media.Md5}{(media.ParentId == null || extension == null ? "" : $".{media.Id}")}{extension ?? $".{GetExtensionFromMime(media.Mime)}"}");

    public string MediaDirectory { get; } = configuration["media:mediaDirectory"]!;
    public string ProducersDirectory { get; } = configuration["media:producersDirectory"]!;
    public string WritersDirectory { get; } = configuration["media:writersDirectory"]!;
    public bool SyncOnBoot { get; } = bool.Parse(configuration["media:syncOnBoot"] ?? "false");
    public bool StopAfterSync { get; } = bool.Parse(configuration["media:stopAfterSync"] ?? "true");
}

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class FileExtensionInfo
{
    public required string Ext { get; init; }
    public required string Mime { get; init; }

    /// <summary>
    /// This sort lets some file extensions take precedence
    /// over others when multiple extensions map to the same MIME type.
    /// For example, both "mp4v" and "mp4" map to "video/mp4".
    /// </summary>
    public required int Order { get; init; }
}