namespace MediaBrowser.Media;

[ApiController, Route("api/[controller]")]
public class MediaController(IFfmpeg ffmpeg, MediaConfig mediaConfig, MediaDbContext context, Nfo nfo) : ControllerBase
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
        var query = request.Apply(context.MediaJoined);

        var count = await query.CountAsync();

        query = request.Sort switch
        {
            Sort.Title => request.Descending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            Sort.CreatedOn => request.Descending ? query.OrderByDescending(m => m.CreatedOn) : query.OrderBy(m => m.CreatedOn),
            Sort.Duration => request.Descending ? query.OrderByDescending(m => m.Duration) : query.OrderBy(m => m.Duration),
            Sort.UserStarRating => request.Descending ? query.OrderByDescending(m => m.UserStarRating) : query.OrderBy(m => m.UserStarRating),
            _ => query
        };

        query = query.Skip(request.Skip);

        if (request.Take != null)
        {
            query = query.Take(request.Take.Value);
        }

        var results = await query.ToListAsync();

        return new()
        {
            Count = count,
            Results = results.Select(it => it.ToReadModel(mediaConfig)).ToArray()
        };
    }

    [HttpGet("cast")]
    public async Task<IReadOnlyList<string>> GetAllCast() =>
        await context.Casts.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("cast/{name}/thumbnail")]
    public ActionResult GetCastThumbnail(string name) => ReadFile(mediaConfig.CastDirectory, $"{name}.jpg");

    [HttpGet("directors")]
    public async Task<IReadOnlyList<string>> GetAllDirectors() =>
        await context.Directors.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("director/{name}/thumbnail")]
    public ActionResult GetDirectorThumbnail(string name) => ReadFile(mediaConfig.DirectorsDirectory, $"{name}.jpg");

    [HttpGet("genres")]
    public async Task<IReadOnlyList<string>> GetAllGenres() =>
        await context.Genres.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("genre/{name}/thumbnail")]
    public ActionResult GetGenreThumbnail(string name) => ReadFile(mediaConfig.GenresDirectory, $"{name}.jpg");

    [HttpGet("producers")]
    public async Task<IReadOnlyList<string>> GetAllProducers() =>
        await context.Producers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("producer/{name}/thumbnail")]
    public ActionResult GetProducerThumbnail(string name) => ReadFile(mediaConfig.ProducersDirectory, $"{name}.jpg");

    [HttpGet("writers")]
    public async Task<IReadOnlyList<string>> GetAllWriters() =>
        await context.Writers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("writer/{name}/thumbnail")]
    public ActionResult GetWriterThumbnail(string name) => ReadFile(mediaConfig.WritersDirectory, $"{name}.jpg");

    [HttpGet("{id:guid}/file")]
    public Task<ActionResult> Stream(Guid id) =>
        ReadFile(id, media => $".{mediaConfig.GetExtensionFromMime(media.Mime)}", media => media.Mime, true);

    [HttpGet("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> StreamFanartThumbnail(Guid id) =>
        ReadFile(id,
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? $".{mediaConfig.GetExtensionFromMime(media.Mime)}" : "-fanart.jpg",
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? media.Mime : "image/jpeg");

    [HttpPost("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> UpdateFanartThumbnail(Guid id, [FromBody] UpdateThumbnailRequest request) =>
        WriteFile(id, "-fanart.jpg", request.At);

    [HttpGet("{id:guid}/file/thumbnail")]
    public Task<ActionResult> StreamThumbnail(Guid id) =>
        ReadFile(id,
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? $".{mediaConfig.GetExtensionFromMime(media.Mime)}" : ".jpg",
            media => media.Mime.StartsWith("image/", StringComparison.InvariantCulture) ? media.Mime : "image/jpeg");

    [HttpPost("{id:guid}/file/thumbnail")]
    public Task<ActionResult> UpdateThumbnail(Guid id, [FromBody] UpdateThumbnailRequest request) =>
        WriteFile(id, ".jpg", request.At, true);

    [HttpPost("{id:guid}/file/thumbnail/file")]
    public async Task<ActionResult> UpdateThumbnail(Guid id, [FromForm] UploadThumbnailRequest request)
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

        var tempFile = Path.GetTempFileName();
        try
        {
            await using (var stream = System.IO.File.Create(tempFile))
            {
                await request.Thumbnail.CopyToAsync(stream);
            }

            if (request.IsPrimary)
            {
                media.Thumbnail = null;
            }

            var extension = request.IsPrimary ? ".jpg" : "-fanart.jpg";

            var thumbnailLocation = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}{extension}");

            if (!await ffmpeg.TryExtractThumbnail(tempFile,
                    outputPath: thumbnailLocation))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            return Ok();
        }
        catch (Exception error)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }
        finally
        {
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);
            }
        }
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

        if (etag)
        {
            var etagValue = $"\"{media.Md5}\"";
            if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == etagValue)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            Response.Headers.ETag = etagValue;
        }

        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        if (Request.Headers.TryGetValue("If-Modified-Since", out var value) &&
            DateTime.TryParse(value, out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), mime(media),
            enableRangeProcessing: true);
    }

    ActionResult ReadFile(string directory, string name)
    {
        var filePath = Path.Combine(directory, name);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        if (Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceValues) &&
            DateTime.TryParse(ifModifiedSinceValues, out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), "image/jpeg");
    }

    async Task<ActionResult> WriteFile(Guid id, string extension, double at, bool isPrimary = false)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        if (isPrimary)
        {
            media.Thumbnail = at;
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
            await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.nfo"));
            await context.SaveChangesAsync();
        }

        return Ok();
    }
}