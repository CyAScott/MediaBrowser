namespace MediaBrowser;

public class DbConfig(IConfiguration configuration)
{
    public bool MigrateOnBoot { get; } = bool.Parse(configuration["db:migrateOnBoot"]!);
    public string ConnectionString { get; } = configuration["db:connectionString"]!;
    /// <summary>
    /// A directory where NFO files are located to import into the DB on boot.
    /// </summary>
    public string ImportOnBootFrom { get; } = configuration["db:importOnBootFrom"]!;
}