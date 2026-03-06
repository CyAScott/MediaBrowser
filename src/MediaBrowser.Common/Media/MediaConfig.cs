namespace MediaBrowser.Media;

public class MediaConfig(IConfiguration configuration)
{
    public string CastDirectory { get; } = configuration["media:castDirectory"]!;
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

    public string MediaDirectory { get; } = configuration["media:mediaDirectory"]!;
    public string ProducersDirectory { get; } = configuration["media:producersDirectory"]!;
    public string WritersDirectory { get; } = configuration["media:writersDirectory"]!;
    public bool SyncOnBoot { get; } = bool.Parse(configuration["media:syncOnBoot"] ?? "false");
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