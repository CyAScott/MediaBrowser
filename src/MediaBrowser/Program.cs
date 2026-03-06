var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

Installer.ConfigureServices(builder.Configuration, builder.Services);

await using var app = builder.Build();
using var cancelTokenSource = new CancellationTokenSource();

Installer.ConfigureApp(app);

await Installer.OnStartup(app.Services, cancelTokenSource);

if (!cancelTokenSource.IsCancellationRequested)
{
    await app.RunAsync();
}
