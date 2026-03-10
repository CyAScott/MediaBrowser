namespace MediaBrowser.Media;

[Equatable, ExcludeFromCodeCoverage(Justification = "POCO")]
public partial class MediaReadModel
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("originalTitle")]
    public required string OriginalTitle { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("mime")]
    public required string Mime { get; init; }

    [JsonPropertyName("size")]
    public required long? Size { get; init; }

    [JsonPropertyName("width")]
    public required int? Width { get; init; }

    [JsonPropertyName("height")]
    public required int? Height { get; init; }

    [JsonPropertyName("duration")]
    public required double? Duration { get; init; }

    [JsonPropertyName("md5")]
    public required string Md5 { get; init; }

    [JsonPropertyName("rating")]
    public required double? Rating { get; init; }

    [JsonPropertyName("userStarRating")]
    public required int? UserStarRating { get; init; }

    [JsonPropertyName("published")]
    public required string Published { get; init; }

    [JsonPropertyName("ctimeMs")]
    public required long CtimeMs { get; init; }

    [JsonPropertyName("mtimeMs")]
    public required long MtimeMs { get; init; }

    [JsonPropertyName("createdOn")]
    public required DateTime? CreatedOn { get; init; }

    [JsonPropertyName("updatedOn")]
    public required DateTime? UpdatedOn { get; init; }

    [JsonPropertyName("ffprobe")]
    public required FfprobeResponse Ffprobe { get; init; }

    [JsonPropertyName("cast"), UnorderedEquality]
    public required IReadOnlyList<string> Cast { get; init; }

    [JsonPropertyName("directors"), UnorderedEquality]
    public required IReadOnlyList<string> Directors { get; init; }

    [JsonPropertyName("genres"), UnorderedEquality]
    public required IReadOnlyList<string> Genres { get; init; }

    [JsonPropertyName("producers"), UnorderedEquality]
    public required IReadOnlyList<string> Producers { get; init; }

    [JsonPropertyName("writers"), UnorderedEquality]
    public required IReadOnlyList<string> Writers { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("thumbnail")]
    public required double? Thumbnail { get; init; }

    [JsonPropertyName("thumbnailUrl")]
    public required string? ThumbnailUrl { get; init; }

    [JsonPropertyName("fanartThumbnail")]
    public required string? FanartThumbnailUrl { get; init; }
}