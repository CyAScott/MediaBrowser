namespace MediaBrowser;

public static class Installer
{
    public const string Version = "v1";

    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Add services to the container
        services.AddControllers();

        // Add global filter for DataAnnotations validation failure
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new ValidationStatus417Filter());
        });

        // Add Swagger services
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(Version, new()
            {
                Title = "MediaBrowser API",
                Version = Version
            });
        });

        MediaInstaller.ConfigureServices(configuration, services);
        ImportInstaller.ConfigureServices(services);
        UserInstaller.ConfigureServices(configuration, services);
    }

    public static void ConfigureApp(IApplicationBuilder app)
    {
        // Configure the HTTP request pipeline
        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseRouting();

        // Enable Swagger middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{Version}/swagger.json", $"MediaBrowser API {Version}");
            c.RoutePrefix = "swagger";
        });

        UserInstaller.ConfigureApp(app);

#pragma warning disable ASP0014
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
#pragma warning restore ASP0014
    }

    public async static Task OnStartup(IServiceProvider services, CancellationTokenSource source)
    {
        await MediaInstaller.OnStartup(services, source);
        await ImportInstaller.OnStartup(services, source);
        await UserInstaller.OnStartup(services, source);
    }
}