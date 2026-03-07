namespace MediaBrowser.Media.Import;

public static class ImportInstaller
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFfmpeg, Ffmpeg>();
        services.AddSingleton<Nfo>();
    }

    public async static Task OnStartup(IServiceProvider services, CancellationToken cancellationToken)
    {
        var mediaConfig = services.GetRequiredService<MediaConfig>();

        if (!mediaConfig.SyncOnBoot)
        {
            return;
        }

        using var scope = services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var log = services.GetRequiredService<ILogger<Nfo>>();
        var nfo = services.GetRequiredService<Nfo>();

        foreach (var nfoLocation in Directory.GetFiles(mediaConfig.MediaDirectory, "*.nfo")
            // Ignore hidden files, which may be temp files created by media management software
            .Where(f => !Path.GetFileName(f).StartsWith('.')))
        {
            try
            {
                var rawXml = await File.ReadAllTextAsync(nfoLocation, Encoding.UTF8, cancellationToken);
                var mediaEntity = nfo.Read(rawXml);

                await db.Media.Where(m => m.Id == mediaEntity.Id).ExecuteDeleteAsync(cancellationToken: cancellationToken);
                db.Media.Add(mediaEntity);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception error)
            {
                log.LogError(error, "{Message}", error.Message);
            }
        }

        if (mediaConfig.StopAfterSync)
        {
            scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }
    }
}