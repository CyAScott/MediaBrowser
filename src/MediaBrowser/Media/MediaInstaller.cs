using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;

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
                connection ??= new MySqlConnection(dbConfig.MySqlConnectionString);
                services.AddDbContext<MediaDbContext, MediaMySqlDbContext>(options =>
                    options.UseMySql(connection, ServerVersion.AutoDetect((MySqlConnection)connection)));
                break;
            case DbType.Postgres:
                connection ??= new NpgsqlConnection(dbConfig.PostgresConnectionString);
                services.AddDbContext<MediaDbContext, MediaPostgresDbContext>(options =>
                    options.UseNpgsql(connection));
                break;
            default:
            case DbType.Sqlite:
                connection ??= new SqliteConnection(dbConfig.SqliteConnectionString);
                services.AddDbContext<MediaDbContext, MediaSqliteDbContext>(options =>
                    options.UseSqlite(connection));
                break;
            case DbType.SqlServer:
                connection ??= new SqlConnection(dbConfig.SqlServerConnectionString);
                services.AddDbContext<MediaDbContext, MediaSqlServerDbContext>(options =>
                    options.UseSqlServer(connection));
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