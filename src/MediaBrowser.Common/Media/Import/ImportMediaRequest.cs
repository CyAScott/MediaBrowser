namespace MediaBrowser.Media.Import;

public class ImportMediaRequest : UpdateMediaRequest
{
    [Range(0, double.MaxValue)]
    public double? Thumbnail { get; init; }
}