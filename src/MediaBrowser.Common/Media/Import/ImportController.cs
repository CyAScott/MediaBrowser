namespace MediaBrowser.Media.Import;

[ApiController, Route("api/[controller]")]
public class ImportController(IFfmpeg ffmpeg, MediaConfig mediaConfig, MediaDbContext context, Nfo nfo) : ControllerBase
{
    [HttpGet("files")]
    public ActionResult<IReadOnlyList<ImportFileInfo>> GetFiles() =>
        !Directory.Exists(mediaConfig.ImportDirectory)
            ? []
            : Directory.GetFiles(mediaConfig.ImportDirectory!, "*.*", SearchOption.TopDirectoryOnly)
                .Select(path => ImportFileInfo.Create(mediaConfig, path))
                .OfType<ImportFileInfo>()
                .OrderBy(it => it.Name)
                .ToList();

    bool FileExists(string name, out string filePath, out FileExtensionInfo fileExtension)
    {
        if (name.StartsWith('.')
            || !name.Contains('.')
            || name.Contains(Path.DirectorySeparatorChar)
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            filePath = null!;
            fileExtension = null!;
            return false;
        }
        filePath = Path.Combine(mediaConfig.ImportDirectory!, name);
        return mediaConfig.ImportExtensions.TryGetValue(Path.GetExtension(name)[1..].ToLowerInvariant(), out fileExtension!)
               && System.IO.File.Exists(filePath);
    }

    [HttpGet("file/{name}")]
    public IActionResult ReadFile(string name)
    {
        if (!FileExists(name, out var filePath, out var fileExtension))
        {
            return NotFound();
        }

        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        if (Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceValue) &&
            DateTime.TryParse(ifModifiedSinceValue, out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), fileExtension.Mime,
            enableRangeProcessing: true);
    }

    [HttpGet("file/{name}/info")]
    public ActionResult<ImportFileInfo> ReadFileInfo(string name)
    {
        if (!FileExists(name, out var filePath, out _))
        {
            return NotFound();
        }

        return ImportFileInfo.Create(mediaConfig, filePath)!;
    }

    [HttpPost("file/{name}")]
    public async Task<ActionResult<MediaReadModel>> Import(string name, [FromBody] ImportMediaRequest request)
    {
        if (!FileExists(name, out var filePath, out _))
        {
            return NotFound();
        }

        var cast = request.Cast.Distinct().ToList();
        var directors = request.Directors.Distinct().ToList();
        var genres = request.Genres.Distinct().ToList();
        var producers = request.Producers.Distinct().ToList();
        var writers = request.Writers.Distinct().ToList();

        if (cast.Concat(directors).Concat(genres).Concat(producers).Concat(writers)
            .Any(it => !IsNameValid(it)))
        {
            return StatusCode(StatusCodes.Status417ExpectationFailed);
        }

        var ffprobe = await ffmpeg.GetMediaInfo(filePath);
        if (ffprobe == null)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var fileInfo = new FileInfo(filePath);

        string hash;
        using (var md5 = MD5.Create())
        await using (var stream = fileInfo.OpenRead())
        {
            hash = Convert.ToHexStringLower(await md5.ComputeHashAsync(stream));
        }

        var media = MediaEntity.Create(
            fileInfo,
            ffprobe.Value.response,
            request,
            hash, ffprobe.Value.mime,
            cast,
            directors,
            genres,
            producers,
            writers,
            thumbnail: request.Thumbnail);
        await context.Media.AddAsync(media);

        await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{hash}.nfo"));

        if (request.Thumbnail != null && ffprobe.Value.mime.StartsWith("video/", StringComparison.InvariantCulture))
        {
            var thumbnailLocation = Path.Combine(mediaConfig.MediaDirectory, $"{hash}.jpg");
            if (!await ffmpeg.TryExtractThumbnail(filePath,
                    outputPath: thumbnailLocation,
                    at: TimeSpan.FromSeconds(request.Thumbnail.Value)))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            System.IO.File.Copy(thumbnailLocation,
                destFileName: Path.Combine(mediaConfig.MediaDirectory, $"{hash}-fanart.jpg"), true);
        }

        await context.SaveChangesAsync();

        var newFilePath = Path.Combine(mediaConfig.MediaDirectory,
            $"{hash}.{mediaConfig.GetExtensionFromMime(ffprobe.Value.mime)}");

        System.IO.File.Move(filePath, newFilePath);

        return media.ToReadModel(mediaConfig);
    }
}