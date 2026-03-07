using static MediaBrowser.Installer;

var builder = CreateHostBuilder(args, []);

using var app = builder.Build();
using var cancelTokenSource = new CancellationTokenSource();

await OnStartup(app.Services, cancelTokenSource.Token);

if (!cancelTokenSource.IsCancellationRequested)
{
    await app.RunAsync(cancelTokenSource.Token);
}
