using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var mediaConfig = new MediaConfig(builder.Configuration);
builder.Services.AddSingleton(mediaConfig);
builder.Services.AddSingleton<IFfmpeg, Ffmpeg>();
builder.Services.AddSingleton<Nfo>();

// Add services to the container
builder.Services.AddControllers();
// Add global filter for DataAnnotations validation failure
builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add(new ValidationStatus417Filter());
});
// Add Swagger services
const string version = "v1";
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(version, new OpenApiInfo { Title = "MediaBrowser API", Version = version });
});

// Configure SQLite database
var dbConfig = new DbConfig(builder.Configuration);
builder.Services.AddDbContext<MediaDbContext>(options =>
    options.UseSqlite(dbConfig.ConnectionString));

// Configure JWT authentication
var userConfig = new UserConfig(builder.Configuration);
builder.Services.AddSingleton(userConfig);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = userConfig.JwtIssuer,
            ValidAudience = userConfig.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userConfig.JwtSecretKey))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

var importLocation = app.Configuration.GetValue<string?>("db:importOnBootFrom");
if (Directory.Exists(importLocation))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    var nfo = app.Services.GetRequiredService<Nfo>();
    var log = app.Services.GetRequiredService<ILogger<Nfo>>();

    foreach (var file in Directory.GetFiles(importLocation, "*.nfo")
        .Where(f => !f.StartsWith(".")))
    {
        try
        {
            var nfoLocation = Path.Combine(importLocation, file);
            var rawXml = await File.ReadAllTextAsync(nfoLocation, Encoding.UTF8);
            var mediaEntity = nfo.Read(rawXml);
        
            await db.Media.Where(m => m.Id == mediaEntity.Id).ExecuteDeleteAsync();
            db.Media.Add(mediaEntity);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message, ex);
        }
    }
    return;
}

// Configure the HTTP request pipeline
app.UseStaticFiles()
    .UseRouting();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"MediaBrowser API {version}");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<JwtCookieMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
#pragma warning restore ASP0014

if (dbConfig.MigrateOnBoot)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    await db.Database.MigrateAsync();
    await db.SaveChangesAsync();
}
await app.RunAsync();

// Custom filter to return 417 on DataAnnotations validation failure
public class ValidationStatus417Filter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new ObjectResult(context.ModelState)
            {
                StatusCode = 417
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
