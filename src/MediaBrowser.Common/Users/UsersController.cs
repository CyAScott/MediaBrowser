using Microsoft.AspNetCore.Authorization;

namespace MediaBrowser.Users;

[ApiController, Route("api/[controller]")]
public class UsersController(UserConfig userConfig, MediaDbContext context) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<UserReadModel>> Login([FromBody] UserLoginRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        Login(user);

        return user.ToReadModel();
    }

    void Login(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(userConfig.JwtSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(userConfig.JwtExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = userConfig.JwtIssuer,
            Audience = userConfig.JwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        Response.Cookies.Append(JwtCookieMiddleware.CookieName, tokenString, new CookieOptions
        {
            HttpOnly = false,
            Expires = tokenDescriptor.Expires,
            Secure = userConfig.UseSecureCookies,
            SameSite = SameSiteMode.Strict
        });
    }

    [HttpPost("logout")]
    public ActionResult Logout()
    {
        Response.Cookies.Append(JwtCookieMiddleware.CookieName, "", new CookieOptions
        {
            HttpOnly = false,
            Expires = DateTime.UtcNow.AddDays(-1),
            Secure = userConfig.UseSecureCookies,
            SameSite = SameSiteMode.Strict
        });
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserReadModel>> Me()
    {
        var user = await context.Users.SingleAsync(u => u.Username == User.Identity!.Name);

        return user.ToReadModel();
    }
    
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult<UserReadModel>> Register([FromBody] UserRegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        Login(user);

        return user.ToReadModel();
    }
}