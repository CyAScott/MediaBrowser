namespace MediaBrowser.Users;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class UserConfig(IConfiguration configuration)
{
    public string JwtAudience { get; } = configuration["jwt:audience"]!;
    public string JwtIssuer { get; } = configuration["jwt:issuer"]!;
    public string JwtSecretKey { get; } = configuration["jwt:secretKey"]!;
    public int JwtExpiryMinutes { get; } = int.Parse(configuration["jwt:expiryMinutes"]!, CultureInfo.InvariantCulture);
    public string? InitialUsername { get; } = configuration["initial:userName"];
    public string? InitialPassword { get; } = configuration["initial:password"];
    public bool UseSecureCookies { get; } = bool.Parse(configuration["cookies:secure"] ?? "true");
}