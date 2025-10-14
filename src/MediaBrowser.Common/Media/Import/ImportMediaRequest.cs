namespace MediaBrowser.Media.Import;

public class ImportMediaRequest : UpdateMediaRequest
{
    [Range(0, double.MaxValue)]
    public required double Thumbnail { get; init; }
}