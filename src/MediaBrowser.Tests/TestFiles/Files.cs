namespace MediaBrowser.TestFiles;

public static class Files
{
    public async static Task Cast(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.cast.jpg")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Director(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.director.jpg")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Genre(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.genre.jpg")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Image(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.image.webp")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Producer(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.producer.jpg")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Writer(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.writer.jpg")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
    public async static Task Mp4(string path)
    {
        await using var stream = typeof(Files).Assembly.GetManifestResourceStream("MediaBrowser.TestFiles.video.mp4")!;
        await using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }
}