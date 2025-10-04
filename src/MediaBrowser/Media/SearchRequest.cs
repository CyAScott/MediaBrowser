namespace MediaBrowser.Media;

public class SearchRequest
{
    public required string? Cast { get; init; }
    public required bool Descending { get; init; }
    public required string? Genres { get; init; }
    public required string? Keywords { get; init; }
    public required Sort Sort { get; init; } = Sort.CreatedOn;
    [Range(0, int.MaxValue)]
    public required int Skip { get; init; }
    [Range(1, 100)]
    public required int Take { get; init; }
}

public enum Sort
{
    Title,
    CreatedOn,
    Duration,
    UserStarRating
}

public class SearchResponse
{
    public required IReadOnlyList<MediaReadModel> Results { get; init; }

    public required int Count { get; init; }
}