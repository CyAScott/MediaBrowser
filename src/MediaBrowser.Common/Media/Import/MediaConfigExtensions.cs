namespace MediaBrowser.Media.Import;

public static class MediaConfigExtensions
{
    public static bool TryToGetFile(this MediaConfig mediaConfig, string name, out (string Path, FileExtensionInfo Extension) file)
    {
        if (mediaConfig.ImportDirectory == null
            || name.StartsWith('.')
            || !name.Contains('.')
            || name.Contains(Path.DirectorySeparatorChar)
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            file = default;
            return false;
        }

        var path = Path.Combine(mediaConfig.ImportDirectory, name);

        if (!mediaConfig.ImportExtensions.TryGetValue(Path.GetExtension(name)[1..].ToLowerInvariant(), out var fileExtension)
            || !File.Exists(path))
        {
            file = default;
            return false;
        }

        file = (path, fileExtension);
        return true;
    }
}