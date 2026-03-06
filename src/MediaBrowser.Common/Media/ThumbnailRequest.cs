namespace MediaBrowser.Media;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class UpdateThumbnailRequest
{
    [Range(0, double.MaxValue)]
    public required double At { get; init; }
}

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class UploadThumbnailRequest
{
    [FromForm(Name = "thumbnail"), Required]
    public required IFormFile Thumbnail { get; init; }

    [FromForm(Name = "is_primary"), Required]
    public required bool IsPrimary { get; init; }
}