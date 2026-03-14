namespace MediaBrowser.Media;

[ApiController, Route("api/[controller]")]
public partial class MediaController(Ffmpeg ffmpeg, MediaConfig mediaConfig, MediaDbContext context, Nfo nfo) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaReadModel>> Get(Guid id)
    {
        var media = await context.MediaJoined.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        return media.ToReadModel(mediaConfig);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MediaReadModel>> Update(Guid id, [FromBody] UpdateMediaRequest request)
    {
        var media = await context.MediaJoined.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        if (request.Cast
            .Concat(request.Directors)
            .Concat(request.Genres)
            .Concat(request.Producers)
            .Concat(request.Writers)
            .Any(it => !IsNameValid(it)))
        {
            return StatusCode(StatusCodes.Status417ExpectationFailed);
        }

        media.Update(request);

        await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.nfo"));

        await context.SaveChangesAsync();

        return media.ToReadModel(mediaConfig);
    }

    [HttpGet("search")]
    public async Task<SearchResponse> Search([FromQuery] SearchRequest request)
    {
        var query = request.CreateQuery(context.MediaJoined);

        var count = await query.CountAsync();

        query = request.ApplySortAndPagination(query);

        var results = await query.ToListAsync();

        return new()
        {
            Count = count,
            Results = results.Select(it => it.ToReadModel(mediaConfig)).ToArray()
        };
    }

    [HttpGet("{id:guid}/file")]
    public Task<ActionResult> Stream(Guid id) =>
        ReadFile(id, media => $".{mediaConfig.GetExtensionFromMime(media.Mime)}", media => media.Mime, true);

    [HttpGet("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> StreamFanartThumbnail(Guid id) =>
        ReadFile(id,
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? $".{mediaConfig.GetExtensionFromMime(media.Mime)}" : "-fanart.jpg",
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? media.Mime : "image/jpeg");

    [HttpPost("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> UpdateFanartThumbnailWithTimestamp(Guid id, [FromBody] UpdateThumbnailRequest request) =>
        UpdateThumbnailWithTimestamp(id, "-fanart.jpg", request.At);

    [HttpGet("{id:guid}/file/thumbnail")]
    public Task<ActionResult> StreamThumbnail(Guid id) =>
        ReadFile(id,
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? $".{mediaConfig.GetExtensionFromMime(media.Mime)}" : ".jpg",
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? media.Mime : "image/jpeg");

    [HttpPost("{id:guid}/file/thumbnail")]
    public Task<ActionResult> UpdateThumbnailWithTimestamp(Guid id, [FromBody] UpdateThumbnailRequest request) =>
        UpdateThumbnailWithTimestamp(id, ".jpg", request.At, true);

    [HttpPost("{id:guid}/file/thumbnail/file")]
    public async Task<ActionResult> UpdateThumbnailWithFile(Guid id, [FromForm] UploadThumbnailRequest request)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        if (media.Mime.StartsWith("image/", StringComparison.InvariantCulture))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var extension = request.IsPrimary ? ".jpg" : "-fanart.jpg";
        var thumbnailLocation = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}{extension}");

        var (result, success) = await UpdateThumbnail(request.Thumbnail, thumbnailLocation);

        if (!success || !request.IsPrimary)
        {
            return result;
        }

        media.Thumbnail = null;
        await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.nfo"));
        await context.SaveChangesAsync();

        return result;
    }

    async Task<ActionResult> ReadFile(Guid id,
        Func<MediaEntity, string> extension,
        Func<MediaEntity, string> mime,
        bool etag = false)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}{extension(media)}");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(filePath);
        if (etag)
        {
            var etagValue = $"\"{media.Md5}\"";
            if (Request.DoEtagsMatch(etagValue, fileInfo.Length))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            Response.Headers.ETag = etagValue;
        }

        if (!Request.WasModifiedSince(fileInfo.LastWriteTimeUtc, fileInfo.Length))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = fileInfo.LastWriteTimeUtc.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), mime(media),
            enableRangeProcessing: true);
    }

    async Task<(ActionResult Result, bool Success)> UpdateThumbnail(IFormFile thumbnail, string thumbnailLocation)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await using (var stream = System.IO.File.Create(tempFile))
            {
                await thumbnail.CopyToAsync(stream);
            }

            if (!await ffmpeg.TryExtractThumbnail(tempFile, outputPath: thumbnailLocation))
            {
                return (StatusCode(StatusCodes.Status406NotAcceptable), false);
            }

            return (Ok(), true);
        }
        finally
        {
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);
            }
        }
    }

    async Task<ActionResult> UpdateThumbnailWithTimestamp(Guid id, string extension, double at, bool isPrimary = false)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        if (!media.Mime.StartsWith("video/", StringComparison.InvariantCulture))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.{mediaConfig.GetExtensionFromMime(media.Mime)}");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var thumbnailLocation = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}{extension}");
        if (!await ffmpeg.TryExtractThumbnail(filePath,
            outputPath: thumbnailLocation,
            at: TimeSpan.FromSeconds(at)))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        // ReSharper disable once InvertIf
        if (isPrimary)
        {
            media.Thumbnail = at;
            await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.nfo"));
            await context.SaveChangesAsync();
        }

        return Ok();
    }
}