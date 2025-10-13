namespace MediaBrowser.Media;

public class UpdateThumbnailRequest
{
    [Range(0, double.MaxValue)]
    public required double At { get; init; }
}