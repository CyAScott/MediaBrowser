using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace MediaBrowser;

public class MediaBrowserWebApplicationFactory(DbType dbType = DbType.Sqlite, TimeSpan? timeout = null) : WebApplicationFactory<Installer>
{
    public CancellationTokenSource CancellationTokenSource { get; } = new(timeout ?? TimeSpan.FromSeconds(30));
    public DbType DbType => dbType;

    readonly List<JsonObject> _configurationFiles =
    [
        new()
        {
            {
                "db", new JsonObject
                {
                    {"type", dbType.ToString()}
                }
            },
            {
                "media", new JsonObject
                {
                    {"stopAfterSync", false}
                }
            }
        }
    ];
    public IReadOnlyList<JsonObject> ConfigurationFiles => _configurationFiles;
    public void AddJsonConfigFile(JsonObject jsonConfiguration) => _configurationFiles.Add(jsonConfiguration);

    public async Task StartServerAsync()
    {
        await this.CleanDatabase(cancellationToken: CancellationTokenSource.Token);
        StartServer();
        await Installer.OnStartup(Services, CancellationTokenSource.Token);
    }
    protected override IHostBuilder CreateHostBuilder() => Installer.CreateHostBuilder([], _configurationFiles.ToArray());
}