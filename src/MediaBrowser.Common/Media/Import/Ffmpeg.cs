using System.Diagnostics;

namespace MediaBrowser.Media.Import;

public interface IFfmpeg
{
    Task<(FfprobeResponse response, string mime)?> GetMediaInfo(string path, CancellationToken cancellationToken = default);
    Task<bool> TryExtractThumbnail(string inputPath, string outputPath, TimeSpan at, CancellationToken cancellationToken = default);
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

    public async Task<bool> TryExtractThumbnail(string inputPath, string outputPath, TimeSpan at, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using var ffprobe = Process.Start("ffmpeg", [
                "-ss", at.ToString(@"hh\:mm\:ss\.fff"),
                "-i", inputPath,
                "-vframes", "1",
                outputPath
            ]);

            await ffprobe.WaitForExitAsync(cancellationToken);
            
            if (!File.Exists(outputPath))
            {
                throw new FileNotFoundException("Failed to extract thumbnail for {Path}", outputPath);
            }

            return true;
        }
        catch (Exception error)
        {
            log.LogError("Failed to extract thumbnail for {Path} at {At}: {error}", inputPath, at, error);
            return false;
        }
    }
}