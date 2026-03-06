namespace MediaBrowser.Media.Import;

public static class ImportInstaller
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFfmpeg, Ffmpeg>();
        services.AddSingleton<Nfo>();
    }

    public async static Task OnStartup(IServiceProvider services, CancellationTokenSource source)
    {
        var mediaConfig = services.GetRequiredService<MediaConfig>();

        if (!mediaConfig.SyncOnBoot)
        {
            return;
        }

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var nfo = services.GetRequiredService<Nfo>();
        var log = services.GetRequiredService<ILogger<Nfo>>();

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