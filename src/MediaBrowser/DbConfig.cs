namespace MediaBrowser;

public class DbConfig(IConfiguration configuration)
{
    public bool MigrateOnBoot { get; } = bool.Parse(configuration["db:migrateOnBoot"]!);
    public string ConnectionString { get; } = configuration["db:connectionString"]!;
}