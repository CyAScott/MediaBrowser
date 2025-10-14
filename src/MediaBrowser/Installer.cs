using MediaBrowser.Media;
using MediaBrowser.Media.Import;
using MediaBrowser.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace MediaBrowser;

public static class Installer
{
    const string _version = "v1";

    public static void OnBoot(WebApplicationBuilder builder)
    {
        // Add services to the container
        builder.Services.AddControllers();

        // Add global filter for DataAnnotations validation failure
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new ValidationStatus417Filter());
        });

        // Add Swagger services
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(_version, new OpenApiInfo { Title = "MediaBrowser API", Version = _version });
        });

        MediaInstaller.OnBoot(builder);
        ImportInstaller.OnBoot(builder);
        UserInstaller.OnBoot(builder);
    }

    public static async Task OnStartup(WebApplication app, CancellationTokenSource source)
    {
        // Configure the HTTP request pipeline
        app.UseStaticFiles()
            .UseRouting();

        // Enable Swagger middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{_version}/swagger.json", $"MediaBrowser API {_version}");
            c.RoutePrefix = "swagger";
        });
        
        await MediaInstaller.OnStartup(app, source);
        await ImportInstaller.OnStartup(app, source);
        await UserInstaller.OnStartup(app, source);

#pragma warning disable ASP0014
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
#pragma warning restore ASP0014
    }
}