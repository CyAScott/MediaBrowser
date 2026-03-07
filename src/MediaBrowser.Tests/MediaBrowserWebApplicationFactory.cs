using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MediaBrowser;

public class MediaBrowserWebApplicationFactory : WebApplicationFactory<Installer>
{
    public MediaBrowserWebApplicationFactory(DbType dbType = DbType.Sqlite, TimeSpan? timeout = null)
    {
        DbType = dbType;
        CancellationTokenSource = new(timeout ?? TimeSpan.FromSeconds(Debugger.IsAttached ? 60 * 60 : 30));

        var tempDirectory = Path.Combine(Path.GetTempPath(), "MediaBrowserTests");
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }
        Directory.CreateDirectory(tempDirectory);

        CastDirectory = Path.Combine(tempDirectory, "cast");
        Directory.CreateDirectory(CastDirectory);
        DirectorsDirectory = Path.Combine(tempDirectory, "directors");
        Directory.CreateDirectory(DirectorsDirectory);
        GenresDirectory = Path.Combine(tempDirectory, "genres");
        Directory.CreateDirectory(GenresDirectory);
        ImportDirectory = Path.Combine(tempDirectory, "import");
        Directory.CreateDirectory(ImportDirectory);
        MediaDirectory = Path.Combine(tempDirectory, "media");
        Directory.CreateDirectory(MediaDirectory);
        ProducersDirectory = Path.Combine(tempDirectory, "producers");
        Directory.CreateDirectory(ProducersDirectory);
        WritersDirectory = Path.Combine(tempDirectory, "writers");
        Directory.CreateDirectory(WritersDirectory);

        ConfigurationFiles =
        [
            new()
            {
                {
                    "cookies", new JsonObject
                    {
                        {"secure", false}
                    }
                },
                {
                    "db", new JsonObject
                    {
                        {"type", dbType.ToString()}
                    }
                },
                {
                    "media", new JsonObject
                    {
                        {"castDirectory", CastDirectory},
                        {"directorsDirectory", DirectorsDirectory},
                        {"genresDirectory", GenresDirectory},
                        {"importDirectory", ImportDirectory},
                        {"mediaDirectory", MediaDirectory},
                        {"producersDirectory", ProducersDirectory},
                        {"writersDirectory", WritersDirectory},
                        {"stopAfterSync", false}
                    }
                }
            }
        ];
    }
    public CancellationTokenSource CancellationTokenSource { get; }
    public DbType DbType { get; }
    public List<JsonObject> ConfigurationFiles { get; }
    public string CastDirectory { get; }
    public string DirectorsDirectory { get; }
    public string GenresDirectory { get; }
    public string ImportDirectory { get; }
    public string MediaDirectory { get; }
    public string ProducersDirectory { get; }
    public string WritersDirectory { get; }

    /// <summary>
    /// Gets the configuration for the application,
    /// which includes the test configuration files defined in this class.
    /// </summary>
    public IConfiguration GetConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.Properties.Add(Installer.CliArgsKey, Array.Empty<string>());
        configurationBuilder.Properties.Add(Installer.TestConfigsKey, ConfigurationFiles.ToArray());
        Installer.ConfigureSettings(configurationBuilder);
        return configurationBuilder.Build();
    }

    public async Task StartServerAsync()
    {
        await this.CleanDatabase(cancellationToken: CancellationTokenSource.Token);
        StartServer();
        await Installer.OnStartup(Services, CancellationTokenSource.Token);
    }
    protected override IHostBuilder CreateHostBuilder() => Installer.CreateHostBuilder([], ConfigurationFiles.ToArray());
}