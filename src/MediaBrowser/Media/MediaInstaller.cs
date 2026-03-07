namespace MediaBrowser.Media;

static class MediaInstaller
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var mediaConfig = new MediaConfig(context.Configuration);
        services.AddSingleton(mediaConfig);

        var dbConfig = new DbConfig(context.Configuration);
        services.AddSingleton(dbConfig);

        switch (dbConfig.DbType)
        {
            case DbType.MySql:
                services.AddDbContext<MediaDbContext, MediaMySqlDbContext>(options =>
                    options.UseMySql(dbConfig.MySqlConnectionString, ServerVersion.AutoDetect(dbConfig.MySqlConnectionString)));
                break;
            case DbType.Postgres:
                services.AddDbContext<MediaDbContext, MediaPostgresDbContext>(options =>
                    options.UseNpgsql(dbConfig.PostgresConnectionString));
                break;
            default:
            case DbType.Sqlite:
                services.AddDbContext<MediaDbContext, MediaSqliteDbContext>(options =>
                    options.UseSqlite(dbConfig.SqliteConnectionString));
                break;
            case DbType.SqlServer:
                services.AddDbContext<MediaDbContext, MediaSqlServerDbContext>(options =>
                    options.UseSqlServer(dbConfig.SqlServerConnectionString));
                break;
        }
    }

    public async static Task OnStartup(IServiceProvider services, CancellationToken cancellationToken)
    {
        var dbConfig = services.GetRequiredService<DbConfig>();
        if (dbConfig.MigrateOnBoot)
        {
            using var scope = services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}