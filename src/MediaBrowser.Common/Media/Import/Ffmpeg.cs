namespace MediaBrowser.Media.Import;

public interface IFfmpeg
{
    Task<(FfprobeResponse response, string mime)?> GetMediaInfo(string path, CancellationToken cancellationToken = default);
    Task<bool> TryExtractThumbnail(string inputPath, string outputPath, TimeSpan? at = null, CancellationToken cancellationToken = default);
}

public class Ffmpeg(ILogger<Ffmpeg> log, MediaConfig mediaConfig) : IFfmpeg
{
    public async Task<(FfprobeResponse response, string mime)?> GetMediaInfo(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            using var ffprobe = Process.Start(new ProcessStartInfo("ffprobe", [
                "-v", "quiet",
                "-print_format", "json",
                "-show_format",
                "-show_streams",
                path
            ])
            {
                RedirectStandardOutput = true
            })!;

            await ffprobe.WaitForExitAsync(cancellationToken);

            var json = await ffprobe.StandardOutput.ReadToEndAsync(cancellationToken);

            var response = JsonSerializer.Deserialize<FfprobeResponse>(json)!;

            var mime = response.Format?.FormatName
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(it => mediaConfig.ImportExtensions.ContainsKey(it))
                .Select(it => mediaConfig.ImportExtensions[it])
                .OrderBy(it => it.Order)
                .FirstOrDefault()?.Mime;

            if (mime == null
                && response.Streams?.Count == 1
                && _imageCodecs.TryGetValue(response.Streams[0].CodecName ?? string.Empty, out var imageMime))
            {
                mime = imageMime;
            }

            return (response, mime ?? throw new ArgumentException("Unable to determine mime type"));
        }
        catch (Exception error)
        {
            log.LogError(error, "Failed to get media info for {Path}", path);
            return null;
        }
    }
    static readonly Dictionary<string, string> _imageCodecs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["png"] = "image/png",
        ["mjpeg"] = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["gif"] = "image/gif",
        ["bmp"] = "image/bmp",
        ["tiff"] = "image/tiff",
        ["webp"] = "image/webp"
    };

    public async Task<bool> TryExtractThumbnail(string inputPath, string outputPath, TimeSpan? at = null, CancellationToken cancellationToken = default)
    {
        var timeAt = at?.ToString(@"hh\:mm\:ss\.f", CultureInfo.InvariantCulture);
        try
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var args = new List<string>();

            if (timeAt != null)
            {
                args.AddRange(["-ss", timeAt]);
            }

            args.AddRange(["-i", inputPath, "-vframes", "1", outputPath]);

            using var ffprobe = Process.Start("ffmpeg", args);

            await ffprobe.WaitForExitAsync(cancellationToken);

            return !File.Exists(outputPath)
                ? throw new FileNotFoundException("Failed to extract thumbnail for {Path}", outputPath)
                : true;
        }
        catch (Exception error)
        {
            log.LogError("Failed to extract thumbnail for {Path} at {TimeAt}: {Error}", inputPath, timeAt, error);
            return false;
        }
    }
}