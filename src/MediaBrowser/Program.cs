using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Add services to the container
builder.Services.AddControllers();

// Configure SQLite database
var dbConfig = new DbConfig(builder.Configuration);
builder.Services.AddDbContext<UserDbContext>(options =>
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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();
