var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

Installer.OnBoot(builder);
MediaInstaller.OnBoot(builder);
ImportInstaller.OnBoot(builder);

using var app = builder.Build();
using var cancelTokenSource = new CancellationTokenSource();

await Installer.OnStartup(app, cancelTokenSource);
await MediaInstaller.OnStartup(app, cancelTokenSource);
await ImportInstaller.OnStartup(app, cancelTokenSource);

if (!cancelTokenSource.IsCancellationRequested)
{
    await app.RunAsync();
}
