namespace MediaBrowser.Media;

public class AddChapterRequest : UpdateMediaRequest
{
    [JsonPropertyName("thumbnail"), Range(0, double.MaxValue)]
    public double? Thumbnail { get; init; }

    [Range(0, double.MaxValue)]
    public required double Duration { get; init; }

    [Range(0, double.MaxValue)]
    public required double Start { get; init; }
}