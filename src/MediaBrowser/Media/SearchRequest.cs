namespace MediaBrowser.Media;

public class SearchRequest
{
    public string? Cast { get; init; }
    public bool Descending { get; init; }
    public string? Genres { get; init; }
    public string? Keywords { get; init; }
    public Sort Sort { get; init; } = Sort.CreatedOn;
    [Range(0, int.MaxValue)]
    public int Skip { get; init; }
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