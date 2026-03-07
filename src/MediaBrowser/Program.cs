using static MediaBrowser.Installer;

var builder = CreateHostBuilder(args, []);

using var app = builder.Build();
var cancellationToken = app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopped;

await OnStartup(app.Services, cancellationToken);

if (!cancellationToken.IsCancellationRequested)
{
    await app.RunAsync(cancellationToken);
}
