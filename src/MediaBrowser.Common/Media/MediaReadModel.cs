namespace MediaBrowser.Media;

public class MediaReadModel
{
    public required Guid Id { get; init; }

    public required string Path { get; init; }

    public required string Title { get; init; }

    public required string OriginalTitle { get; init; }

    public required string Description { get; init; }

    public required string Mime { get; init; }

    public required long? Size { get; init; }

    public required int? Width { get; init; }

    public required int? Height { get; init; }

    public required double? Duration { get; init; }

    public required string Md5 { get; init; }

    public required double? Rating { get; init; }

    public required int? UserStarRating { get; init; }

    public required string Published { get; init; }

    public required long CtimeMs { get; init; }

    public required long MtimeMs { get; init; }

    public required DateTime? CreatedOn { get; init; }

    public required DateTime? UpdatedOn { get; init; }

    public required FfprobeResponse Ffprobe { get; init; }
    
    public required IReadOnlyList<string> Cast { get; init; }
    
    public required IReadOnlyList<string> Directors { get; init; }
    
    public required IReadOnlyList<string> Genres { get; init; }
    
    public required IReadOnlyList<string> Producers { get; init; }
    
    public required IReadOnlyList<string> Writers { get; init; }
    
    public required string Url { get; init; }

    public required double? Thumbnail { get; init; }
    
    public required string? ThumbnailUrl { get; init; }
    
    public required string? FanartThumbnailUrl { get; init; }
}