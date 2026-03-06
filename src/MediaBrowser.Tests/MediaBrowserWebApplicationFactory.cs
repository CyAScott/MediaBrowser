using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MediaBrowser.Tests;

public class MediaBrowserWebApplicationFactory : WebApplicationFactory<Program>
{
    readonly List<JsonObject> _configurationFiles = [];
    public void AddJsonConfigFile(JsonObject jsonConfiguration) => _configurationFiles.Add(jsonConfiguration);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder();
        foreach (var bytes in _configurationFiles
            .Select(jsonConfiguration => jsonConfiguration.ToJsonString())
            .Select(json => Encoding.UTF8.GetBytes(json)))
        {
            configurationBuilder.AddJsonStream(new MemoryStream(bytes, false));
        }
    }
}