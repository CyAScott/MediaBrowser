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

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        var ids = new List<Guid>
        {
            media.Id
        };

        IEnumerable<string> GetFiles(MediaEntity mediaEntity)
        {
            yield return mediaConfig.MediaFileLocation(media, ".nfo");
            if (!mediaEntity.IsImage())
            {
                yield return mediaConfig.MediaFileLocation(mediaEntity, ".jpg");
                yield return mediaConfig.MediaFileLocation(mediaEntity, "-fanart.jpg");
            }
        }

        var files = GetFiles(media).ToList();

        if (media.ParentId == null)
        {
            files.Add(mediaConfig.MediaFileLocation(media));

            foreach (var chapter in await context.Media.Where(m => m.ParentId == id).ToListAsync())
            {
                ids.Add(chapter.Id);
                files.AddRange(GetFiles(chapter));
            }
        }

        await context.MediaJoined.Where(m => ids.Contains(m.Id)).ExecuteDeleteAsync();

        foreach (var file in files)
        {
            if (System.IO.File.Exists(file))
            {
                var deletedFile = Path.Combine(mediaConfig.DeletedDirectory, Path.GetFileName(file));
                System.IO.File.Move(file, deletedFile);
            }
        }

        await context.SaveChangesAsync();

        return Ok();
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

        await nfo.Save(media, mediaConfig.MediaFileLocation(media, ".nfo"));

        await context.SaveChangesAsync();

        return media.ToReadModel(mediaConfig);
    }

    [HttpPost("{id:guid}/chapters")]
    public async Task<ActionResult<MediaReadModel>> AddChapter(Guid id, [FromBody] AddChapterRequest request)
    {
        var media = await context.MediaJoined.Where(m => m.Id == id && m.ParentId == null).FirstOrDefaultAsync();
        if (media == null || media.IsImage())
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

        if (request.Start + request.Duration > media.Duration)
        {
            return StatusCode(StatusCodes.Status416RangeNotSatisfiable);
        }

        var chapter = media.CreateChapter(request);

        await nfo.Save(chapter, mediaConfig.MediaFileLocation(chapter, ".nfo"));

        await context.AddAsync(chapter);

        await context.SaveChangesAsync();

        if (request.Thumbnail != null && chapter.Mime.StartsWith("video/", StringComparison.InvariantCulture))
        {
            var thumbnailLocation = mediaConfig.MediaFileLocation(chapter, ".jpg");
            if (!await ffmpeg.TryExtractThumbnail(mediaConfig.MediaFileLocation(chapter),
                    outputPath: thumbnailLocation,
                    at: TimeSpan.FromSeconds(request.Thumbnail.Value)))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            System.IO.File.Copy(thumbnailLocation,
                destFileName: mediaConfig.MediaFileLocation(chapter, "-fanart.jpg"), true);
        }

        return chapter.ToReadModel(mediaConfig);
    }

    [HttpGet("search")]
    public async Task<SearchResponse> Search([FromQuery] SearchRequest request)
    {
        var query = request.CreateQuery(context.MediaJoined);

        var count = await query.CountAsync();

        query = await request.ApplySortAndPagination(context, query);

        var results = await query.ToListAsync();

        return new()
        {
            Count = count,
            Results = results.Select(it => it.ToReadModel(mediaConfig)).ToArray()
        };
    }

    [HttpGet("{id:guid}/file")]
    public Task<ActionResult> Stream(Guid id) =>
        ReadFile(id, media => mediaConfig.MediaFileLocation(media), etag: true);

    [HttpGet("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> StreamFanartThumbnail(Guid id) =>
        ReadFile(id,
            media => media.IsImage() ? mediaConfig.MediaFileLocation(media) : mediaConfig.MediaFileLocation(media, "-fanart.jpg"),
            media => media.IsImage() ? media.Mime : "image/jpeg");

    [HttpPost("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> UpdateFanartThumbnailWithTimestamp(Guid id, [FromBody] UpdateThumbnailRequest request) =>
        UpdateThumbnailWithTimestamp(id, "-fanart.jpg", request.At);

    [HttpGet("{id:guid}/file/thumbnail")]
    public Task<ActionResult> StreamThumbnail(Guid id) =>
        ReadFile(id,
            media => media.IsImage() ? mediaConfig.MediaFileLocation(media) : mediaConfig.MediaFileLocation(media, ".jpg"),
            media => media.IsImage() ? media.Mime : "image/jpeg");

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

        if (media.IsImage())
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var thumbnailLocation = mediaConfig.MediaFileLocation(media, request.IsPrimary ? ".jpg" : "-fanart.jpg");

        var (result, success) = await UpdateThumbnail(request.Thumbnail, thumbnailLocation);

        if (!success || !request.IsPrimary)
        {
            return result;
        }

        media.Thumbnail = null;
        await nfo.Save(media, mediaConfig.MediaFileLocation(media, ".nfo"));
        await context.SaveChangesAsync();

        return result;
    }

    async Task<ActionResult> ReadFile(Guid id,
        Func<MediaEntity, string> filePathFactory,
        Func<MediaEntity, string>? mimeFactory = null, bool etag = false)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }

        var filePath = filePathFactory(media);
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

        mimeFactory ??= it => it.Mime;

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), mimeFactory(media),
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

        if (media.IsImage())
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var filePath = mediaConfig.MediaFileLocation(media);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var thumbnailLocation = mediaConfig.MediaFileLocation(media, extension);
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
            await nfo.Save(media, mediaConfig.MediaFileLocation(media, ".nfo"));
            await context.SaveChangesAsync();
        }

        return Ok();
    }
}