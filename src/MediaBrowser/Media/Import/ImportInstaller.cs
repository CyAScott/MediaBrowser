namespace MediaBrowser.Media.Import;

static class ImportInstaller
{
    public static void OnBoot(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFfmpeg, Ffmpeg>();
        builder.Services.AddSingleton<Nfo>();
    }

    public static async Task OnStartup(WebApplication app, CancellationTokenSource source)
    {
        var dbConfig = app.Services.GetRequiredService<DbConfig>();

        if (!Directory.Exists(dbConfig.ImportOnBootFrom))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var nfo = app.Services.GetRequiredService<Nfo>();
        var log = app.Services.GetRequiredService<ILogger<Nfo>>();

        foreach (var file in Directory.GetFiles(dbConfig.ImportOnBootFrom, "*.nfo")
            .Where(f => !Path.GetFileName(f).StartsWith(".")))
        {
            try
            {
                var nfoLocation = Path.Combine(dbConfig.ImportOnBootFrom, file);
                var rawXml = await File.ReadAllTextAsync(nfoLocation, Encoding.UTF8, source.Token);
                var mediaEntity = nfo.Read(rawXml);
        
                await db.Media.Where(m => m.Id == mediaEntity.Id).ExecuteDeleteAsync();
                db.Media.Add(mediaEntity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
            }
        }

        await source.CancelAsync();
    }
}