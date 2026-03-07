using Microsoft.Extensions.Hosting;

namespace MediaBrowser.Users;

public static class UserInstaller
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Configure JWT authentication
        var userConfig = new UserConfig(context.Configuration);
        services.AddSingleton(userConfig);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
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
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    public static void ConfigureApp(IApplicationBuilder app)
    {
        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public async static Task OnStartup(IServiceProvider services, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var userConfig = services.GetRequiredService<UserConfig>();
        if (!string.IsNullOrEmpty(userConfig.InitialUsername)
            && !string.IsNullOrEmpty(userConfig.InitialPassword))
        {
            using var scope = services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            if (!await db.Users.AnyAsync(u => u.Username == userConfig.InitialUsername, cancellationToken))
            {
                var user = new UserEntity
                {
                    Id = Guid.CreateVersion7(),
                    Username = userConfig.InitialUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userConfig.InitialPassword)
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}