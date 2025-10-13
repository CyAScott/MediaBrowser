namespace MediaBrowser;

public class DbConfig(IConfiguration configuration)
{
    public bool MigrateOnBoot { get; } = bool.Parse(configuration["db:migrateOnBoot"]!);
    public string MySqlConnectionString { get; } = configuration["db:mySqlConnectionString"]!;
    public string PostgresConnectionString { get; } = configuration["db:postgresConnectionString"]!;
    public string SqliteConnectionString { get; } = configuration["db:sqliteConnectionString"]!;
    public string SqlServerConnectionString { get; } = configuration["db:sqlServerConnectionString"]!;
    public DbType DbType { get; } = Enum.Parse<DbType>(configuration["db:type"] ?? nameof(DbType.Sqlite), true);
    /// <summary>
    /// A directory where NFO files are located to import into the DB on boot.
    /// </summary>
    public string ImportOnBootFrom { get; } = configuration["db:importOnBootFrom"]!;
}

public enum DbType
{
    MySql,
    Postgres,
    Sqlite,
    SqlServer
}