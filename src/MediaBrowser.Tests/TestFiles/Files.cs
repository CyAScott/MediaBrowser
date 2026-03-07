namespace MediaBrowser.TestFiles;

public static class Files
{
    public async static Task Mp4(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.test.mp4")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
}