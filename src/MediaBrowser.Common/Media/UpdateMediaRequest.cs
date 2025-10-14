namespace MediaBrowser.Media;

public class UpdateMediaRequest
{
    public required string Title { get; init; }

    public required string OriginalTitle { get; init; }

    public required string Description { get; init; }

    [Range(0, 10)]
    public required double? Rating { get; init; }

    [Range(0, 5)]
    public required int? UserStarRating { get; init; }
    
    public required IReadOnlyList<string> Cast { get; init; }
    
    public required IReadOnlyList<string> Directors { get; init; }
    
    public required IReadOnlyList<string> Genres { get; init; }
    
    public required IReadOnlyList<string> Producers { get; init; }
    
    public required IReadOnlyList<string> Writers { get; init; }
}