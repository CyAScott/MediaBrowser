using Microsoft.EntityFrameworkCore;

namespace MediaBrowser.Media;

static class MediaInstaller
{
    public static void OnBoot(WebApplicationBuilder builder)
    {
        var mediaConfig = new MediaConfig(builder.Configuration);
        builder.Services.AddSingleton(mediaConfig);

        var dbConfig = new DbConfig(builder.Configuration);
        builder.Services.AddSingleton(dbConfig);

        switch (dbConfig.DbType)
        {
            case DbType.MySql:
                builder.Services.AddDbContext<MediaDbContext, MediaMySqlDbContext>(options =>
                    options.UseMySql(dbConfig.MySqlConnectionString, ServerVersion.AutoDetect(dbConfig.MySqlConnectionString)));
                break;
            case DbType.Postgres:
                builder.Services.AddDbContext<MediaDbContext, MediaPostgresDbContext>(options =>
                    options.UseNpgsql(dbConfig.PostgresConnectionString));
                break;
            default:
            case DbType.Sqlite:
                builder.Services.AddDbContext<MediaDbContext, MediaSqliteDbContext>(options =>
                    options.UseSqlite(dbConfig.SqliteConnectionString));
                break;
            case DbType.SqlServer:
                builder.Services.AddDbContext<MediaDbContext, MediaSqlServerDbContext>(options =>
                    options.UseSqlServer(dbConfig.SqlServerConnectionString));
                break;
        }
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