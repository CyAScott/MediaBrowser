using System.Diagnostics;

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
                .FirstOrDefault()?.Mime ?? throw new ArgumentException("Unable to determine mime type");

            return (response, mime);
        }
        catch (Exception error)
        {
            log.LogError(error, "Failed to get media info for {Path}", path);
            return null;
        }
    }

    public async Task<bool> TryExtractThumbnail(string inputPath, string outputPath, TimeSpan? at = null, CancellationToken cancellationToken = default)
    {
        var timeAt = at?.ToString(@"hh\:mm\:ss\.fff");
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
            
            args.AddRange([
                "-i", inputPath,
                "-vframes", "1",
                outputPath
            ]);

            using var ffprobe = Process.Start("ffmpeg", args);

            await ffprobe.WaitForExitAsync(cancellationToken);
            
            if (!File.Exists(outputPath))
            {
                throw new FileNotFoundException("Failed to extract thumbnail for {Path}", outputPath);
            }

            return true;
        }
        catch (Exception error)
        {
            log.LogError("Failed to extract thumbnail for {Path} at {timeAt}: {error}", inputPath, timeAt, error);
            return false;
        }
    }
}