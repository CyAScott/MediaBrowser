namespace MediaBrowser.Media;

partial class MediaController
{
    [HttpGet("{tagType}"), HttpGet("{tagType}s")]
    public async Task<IReadOnlyList<string>> GetAll(TagType tagType) => tagType switch
    {
        TagType.Cast => await context.Casts.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync(),
        TagType.Director => await context.Directors.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync(),
        TagType.Genre => await context.Genres.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync(),
        TagType.Producer => await context.Producers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync(),
        _ => await context.Writers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync()
    };

    string GetTagDirectory(TagType tagType) => tagType switch
    {
        TagType.Cast => mediaConfig.CastDirectory,
        TagType.Director => mediaConfig.DirectorsDirectory,
        TagType.Genre => mediaConfig.GenresDirectory,
        TagType.Producer => mediaConfig.ProducersDirectory,
        _ => mediaConfig.WritersDirectory
    };

    [HttpGet("{tagType}/{name}/thumbnail")]
    public ActionResult GetThumbnail(TagType tagType, string name)
    {
        var filePath = Path.Combine(GetTagDirectory(tagType), $"{name}.jpg");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(filePath);
        if (!Request.WasModifiedSince(fileInfo.LastWriteTimeUtc, fileInfo.Length))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = fileInfo.LastWriteTimeUtc.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), "image/jpeg");
    }

    [HttpPost("{tagType}/{name}/thumbnail")]
    public async Task<ActionResult> SetThumbnail(TagType tagType, string name, [FromForm] SetTagThumbnailRequest request)
    {
        if (!IsNameValid(name))
        {
            return StatusCode(StatusCodes.Status417ExpectationFailed);
        }

        var thumbnailLocation = Path.Combine(GetTagDirectory(tagType), $"{name}.jpg");

        var (result, _) = await UpdateThumbnail(request.Thumbnail, thumbnailLocation);

        return result;
    }
}