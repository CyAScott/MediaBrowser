namespace MediaBrowser.Media.Import;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class AddFileRequest
{
    [FromForm(Name = "file"), Required]
    public required IFormFile File { get; init; }
}