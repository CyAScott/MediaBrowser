namespace MediaBrowser.Media.Import;

[Equatable, ExcludeFromCodeCoverage(Justification = "POCO")]
public partial class ImportMediaRequest : UpdateMediaRequest
{
    [JsonPropertyName("thumbnail"), Range(0, double.MaxValue)]
    public double? Thumbnail { get; init; }
}