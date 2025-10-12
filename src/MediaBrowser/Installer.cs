using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace MediaBrowser;

static class Installer
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
    }

    public static Task OnStartup(WebApplication app, CancellationTokenSource source)
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

        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
#pragma warning disable ASP0014
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
#pragma warning restore ASP0014
        
        return Task.CompletedTask;
    }
}