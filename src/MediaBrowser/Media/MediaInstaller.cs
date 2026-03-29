namespace MediaBrowser.Media;

static class MediaInstaller
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services, DbConnection? connection)
    {
        var mediaConfig = new MediaConfig(context.Configuration);
        services.AddSingleton(mediaConfig);

        var dbConfig = new DbConfig(context.Configuration);
        services.AddSingleton(dbConfig);

        switch (dbConfig.DbType)
        {
            case DbType.MySql:
                services.AddDbContext<MediaDbContext, MediaMySqlDbContext>(options =>
                {
                    if (connection == null)
                    {
                        options.UseMySql(dbConfig.MySqlConnectionString, ServerVersion.AutoDetect(dbConfig.MySqlConnectionString));
                    }
                    else
                    {
                        options.UseMySql(connection, ServerVersion.AutoDetect((MySqlConnection)connection));
                    }
                });
                break;
            case DbType.Postgres:
                services.AddDbContext<MediaDbContext, MediaPostgresDbContext>(options =>
                {
                    if (connection == null)
                    {
                        options.UseNpgsql(dbConfig.PostgresConnectionString);
                    }
                    else
                    {
                        options.UseNpgsql(connection);
                    }
                });
                break;
            default:
            case DbType.Sqlite:
                services.AddDbContext<MediaDbContext, MediaSqliteDbContext>(options =>
                {
                    if (connection == null)
                    {
                        options.UseSqlite(dbConfig.SqliteConnectionString).AddInterceptors(new SqliteUdfInterceptor());
                    }
                    else
                    {
                        options.UseSqlite(connection);
                    }
                });
                break;
            case DbType.SqlServer:
                services.AddDbContext<MediaDbContext, MediaSqlServerDbContext>(options =>
                {
                    if (connection == null)
                    {
                        options.UseSqlServer(dbConfig.SqlServerConnectionString);
                    }
                    else
                    {
                        options.UseSqlServer(connection);
                    }
                });
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