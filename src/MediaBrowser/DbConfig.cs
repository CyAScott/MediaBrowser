namespace MediaBrowser;

public class DbConfig(IConfiguration configuration)
{
    public string ConnectionString { get; } = configuration["db:connectionString"]!;
}