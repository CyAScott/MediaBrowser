namespace MediaBrowser.Media.Import;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class ImportMediaRequest : UpdateMediaRequest
{
    [Range(0, double.MaxValue)]
    public double? Thumbnail { get; init; }
}