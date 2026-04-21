using Microsoft.AspNetCore.Http.Features;

namespace MediaBrowser;

public class Installer
{
    public const string CliArgsKey = "CliArgs", TestConfigsKey = "TestConfigs";
    public static IHostBuilder CreateHostBuilder(string[] args, IReadOnlyList<JsonObject> configs, string version, DbConnection? connection = null)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.Properties.Add(CliArgsKey, args);
                configurationBuilder.Properties.Add(TestConfigsKey, configs);
            })
            .ConfigureAppConfiguration(ConfigureSettings)
            .ConfigureServices((_, services) => ConfigureServices(services, version))
            .ConfigureServices((context, services) =>
                MediaInstaller.ConfigureServices(context, services, connection))
            .ConfigureServices(ImportInstaller.ConfigureServices)
            .ConfigureServices(UserInstaller.ConfigureServices)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.Configure(app => ConfigureApp(app, version)));

        return builder;
    }

    public static void ConfigureSettings(IConfigurationBuilder configurationBuilder)
    {
        // Add configuration sources to the host builder
        configurationBuilder.AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine((string[])configurationBuilder.Properties[CliArgsKey]);

        // load optional test configurations
        foreach (var bytes in ((IEnumerable<JsonObject>)configurationBuilder.Properties[TestConfigsKey])
                     .Select(jsonConfiguration => jsonConfiguration.ToJsonString())
                     .Select(json => Encoding.UTF8.GetBytes(json)))
        {
            configurationBuilder.AddJsonStream(new MemoryStream(bytes, false));
        }
    }

    static void ConfigureServices(IServiceCollection services, string version)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue;
        });

        // Add services to the container
        services.AddControllers()
            .AddApplicationPart(typeof(DbConfig).Assembly)
            .AddApplicationPart(typeof(Installer).Assembly);

        // Add global filter for DataAnnotations validation failure
        services.Configure<MvcOptions>(options => options.Filters.Add(new ValidationStatus417Filter()));

        // Add Swagger services
        services.AddSwaggerGen(options => options.SwaggerDoc(version, new()
        {
            Title = "MediaBrowser API",
            Version = version
        }));
    }

    static void ConfigureApp(IApplicationBuilder app, string version)
    {
        // Configure the HTTP request pipeline
        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseRouting();

        // Enable Swagger middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"MediaBrowser API {version}");
            c.RoutePrefix = "swagger";
        });

        UserInstaller.ConfigureApp(app);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    public async static Task OnStartup(IServiceProvider services, CancellationToken cancellationToken)
    {
        await MediaInstaller.OnStartup(services, cancellationToken);
        await ImportInstaller.OnStartup(services, cancellationToken);
        await UserInstaller.OnStartup(services, cancellationToken);
    }
}