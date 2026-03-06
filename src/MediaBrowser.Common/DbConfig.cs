namespace MediaBrowser;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class DbConfig(IConfiguration configuration)
{
    public bool MigrateOnBoot { get; } = bool.Parse(configuration["db:migrateOnBoot"]!);
    public string MySqlConnectionString { get; } = configuration["db:mySqlConnectionString"]!;
    public string PostgresConnectionString { get; } = configuration["db:postgresConnectionString"]!;
    public string SqliteConnectionString { get; } = configuration["db:sqliteConnectionString"]!;
    public string SqlServerConnectionString { get; } = configuration["db:sqlServerConnectionString"]!;
    public DbType DbType { get; } = Enum.Parse<DbType>(configuration["db:type"] ?? nameof(DbType.Sqlite), true);
}

public enum DbType
{
    MySql,
    Postgres,
    Sqlite,
    SqlServer
}