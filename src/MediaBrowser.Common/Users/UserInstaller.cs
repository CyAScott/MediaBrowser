using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MediaBrowser.Users;

public static class UserInstaller
{
    public static void OnBoot(WebApplicationBuilder builder)
    {
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
    
    public static async Task OnStartup(WebApplication app, CancellationTokenSource source)
    {
        if (source.IsCancellationRequested)
        {
            return;
        }

        var userConfig = app.Services.GetRequiredService<UserConfig>();
        if (!string.IsNullOrEmpty(userConfig.InitialUsername)
            && !string.IsNullOrEmpty(userConfig.InitialPassword))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            if (!await db.Users.AnyAsync(u => u.Username == userConfig.InitialUsername, source.Token))
            {
                var user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Username = userConfig.InitialUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userConfig.InitialPassword)
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(source.Token);
            }
        }

        app.UseMiddleware<JwtCookieMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}