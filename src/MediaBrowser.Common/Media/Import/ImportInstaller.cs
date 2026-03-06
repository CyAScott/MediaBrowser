namespace MediaBrowser.Media.Import;

public static class ImportInstaller
{
    public static void OnBoot(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFfmpeg, Ffmpeg>();
        builder.Services.AddSingleton<Nfo>();
    }

    public async static Task OnStartup(WebApplication app, CancellationTokenSource source)
    {
        var mediaConfig = app.Services.GetRequiredService<MediaConfig>();

        if (!mediaConfig.SyncOnBoot)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var nfo = app.Services.GetRequiredService<Nfo>();
        var log = app.Services.GetRequiredService<ILogger<Nfo>>();

        foreach (var nfoLocation in Directory.GetFiles(mediaConfig.MediaDirectory, "*.nfo")
            .Where(f => !Path.GetFileName(f).StartsWith('.')))
        {
            try
            {
                var rawXml = await File.ReadAllTextAsync(nfoLocation, Encoding.UTF8, source.Token);
                var mediaEntity = nfo.Read(rawXml);

                await db.Media.Where(m => m.Id == mediaEntity.Id).ExecuteDeleteAsync();
                db.Media.Add(mediaEntity);
                await db.SaveChangesAsync();
            }
            catch (Exception error)
            {
#pragma warning disable CA2254
                log.LogError(error, error.Message);
#pragma warning restore CA2254
            }
        }

        await source.CancelAsync();
    }
}