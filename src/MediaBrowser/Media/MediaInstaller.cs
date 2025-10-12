namespace MediaBrowser.Media;

static class MediaInstaller
{
    public static void OnBoot(WebApplicationBuilder builder)
    {
        var mediaConfig = new MediaConfig(builder.Configuration);
        builder.Services.AddSingleton(mediaConfig);

        var dbConfig = new DbConfig(builder.Configuration);
        builder.Services.AddSingleton(dbConfig);

        builder.Services.AddDbContext<MediaDbContext>(options =>
            options.UseSqlite(dbConfig.ConnectionString));
    }

    public static async Task OnStartup(WebApplication app, CancellationTokenSource source)
    {
        var dbConfig = app.Services.GetRequiredService<DbConfig>();
        if (dbConfig.MigrateOnBoot)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            await db.Database.MigrateAsync(cancellationToken: source.Token);
            await db.SaveChangesAsync(source.Token);
        }
    }
}