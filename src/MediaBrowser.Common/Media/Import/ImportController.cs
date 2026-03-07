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

    [HttpGet("file/{name}")]
    public IActionResult ReadFile(string name)
    {
        if (!mediaConfig.TryToGetFile(name, out var file))
        {
            return NotFound();
        }

        var lastModified = System.IO.File.GetLastWriteTimeUtc(file.Path);
        if (!Request.WasModifiedSince(lastModified))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers.LastModified = lastModified.ToString("R");

        return File(new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read), file.Extension.Mime,
            enableRangeProcessing: true);
    }

    [HttpGet("file/{name}/info")]
    public ActionResult<ImportFileInfo> ReadFileInfo(string name) =>
        !mediaConfig.TryToGetFile(name, out var file)
            ? NotFound()
            : ImportFileInfo.Create(mediaConfig, file.Path)!;

    [HttpPost("file/{name}")]
    public async Task<ActionResult<MediaReadModel>> Import(string name, [FromBody] ImportMediaRequest request)
    {
        if (!mediaConfig.TryToGetFile(name, out var file))
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

        var ffprobe = await ffmpeg.GetMediaInfo(file.Path);
        if (ffprobe == null)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        var fileInfo = new FileInfo(file.Path);

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
            if (!await ffmpeg.TryExtractThumbnail(file.Path,
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

        System.IO.File.Move(file.Path, newFilePath);

        return media.ToReadModel(mediaConfig);
    }
}