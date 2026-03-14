namespace MediaBrowser.Media;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class SetTagThumbnailRequest
{
    [FromForm(Name = "thumbnail"), Required]
    public required IFormFile Thumbnail { get; init; }
}