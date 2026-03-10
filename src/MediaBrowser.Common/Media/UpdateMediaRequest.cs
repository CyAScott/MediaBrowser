namespace MediaBrowser.Media;

[Equatable, ExcludeFromCodeCoverage(Justification = "POCO")]
public partial class UpdateMediaRequest
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("originalTitle")]
    public required string OriginalTitle { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("rating"), Range(0, 10)]
    public double? Rating { get; init; }

    [JsonPropertyName("userStarRating"), Range(0, 5)]
    public int? UserStarRating { get; init; }

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
}