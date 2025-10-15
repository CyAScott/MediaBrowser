using System.Security.Cryptography;

namespace MediaBrowser.Media.Import;

[ApiController, Route("api/[controller]")]
public class ImportController(IFfmpeg ffmpeg, MediaConfig mediaConfig, MediaDbContext context, Nfo nfo) : ControllerBase
{
    [HttpGet("files")]
    public ActionResult<IReadOnlyList<string>> GetFiles() =>
        !Directory.Exists(mediaConfig.ImportDirectory)
            ? []
            : Directory.GetFiles(mediaConfig.ImportDirectory!, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file =>
                    !file.StartsWith(".") &&
                    file.Contains('.') &&
                    mediaConfig.ImportExtensions.ContainsKey(Path.GetExtension(file).Substring(1)))
                .Select(file => Path.Combine(mediaConfig.ImportDirectory, file))
                .OrderBy(name => name)
                .ToList();

    bool FileExists(string name, out string filePath, out FileExtensionInfo fileExtension)
    {
        if (name.StartsWith(".") 
            || !name.Contains('.')
            || name.Contains(Path.DirectorySeparatorChar)
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            filePath = null!;
            fileExtension = null!;
            return false;
        }
        filePath = Path.Combine(mediaConfig.ImportDirectory!, name);
        return mediaConfig.ImportExtensions.TryGetValue(Path.GetExtension(name).Substring(1).ToLower(), out fileExtension!)
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
        if (Request.Headers.ContainsKey("If-Modified-Since") &&
            DateTime.TryParse(Request.Headers["If-Modified-Since"], out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers["Last-Modified"] = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), fileExtension.Mime,
            enableRangeProcessing: true);
    }

    [HttpPost("file/{name}")]
    public async Task<ActionResult> Import(string name, [FromBody] ImportMediaRequest request)
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
        using (var stream = fileInfo.OpenRead())
        {
            hash = BitConverter.ToString(await md5.ComputeHashAsync(stream)).Replace("-", "").ToLowerInvariant();
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
            writers);
        await context.Media.AddAsync(media);
        
        await nfo.Save(media, Path.Combine(mediaConfig.MediaDirectory, $"{hash}.nfo"));

        var thumbnailLocation = Path.Combine(mediaConfig.MediaDirectory, $"{hash}.jpg");
        if (!await ffmpeg.TryExtractThumbnail(filePath,
            outputPath: thumbnailLocation,
            at: TimeSpan.FromSeconds(request.Thumbnail)))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }
        
        System.IO.File.Copy(thumbnailLocation, 
            destFileName: Path.Combine(mediaConfig.MediaDirectory, $"{hash}-fanart.jpg"), true);
        
        await context.SaveChangesAsync();
        
        var newFilePath = Path.Combine(mediaConfig.MediaDirectory,
            $"{hash}.{mediaConfig.GetExtensionFromMime(ffprobe.Value.mime)}");
        
        System.IO.File.Move(filePath, newFilePath);

        return Ok();
    }
}