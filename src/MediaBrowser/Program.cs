using static MediaBrowser.Installer;

namespace MediaBrowser;

[ExcludeFromCodeCoverage(Justification = "This is the entry point of the application and is not easily testable.")]
public static class Program
{
    public async static Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args, []);

        using var app = builder.Build();
        var cancellationToken = app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopped;

        await OnStartup(app.Services, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
        {
            await app.RunAsync(cancellationToken);
        }
    }
}
