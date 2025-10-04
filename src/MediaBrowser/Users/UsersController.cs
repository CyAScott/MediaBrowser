namespace MediaBrowser.Users;

[ApiController, Route("api/[controller]")]
public class UsersController(UserConfig userConfig, MediaDbContext context) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserReadModel>> Login([FromBody] UserLoginRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == request.UserName);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(userConfig.JwtSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(userConfig.JwtExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = userConfig.JwtIssuer,
            Audience = userConfig.JwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        Response.Cookies.Append("jwt", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Expires = tokenDescriptor.Expires,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return user.ToReadModel();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserReadModel>> Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var user = await context.Users.SingleAsync(u => u.UserName == User.Identity.Name);

        return user.ToReadModel();
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserReadModel>> Register([FromBody] UserRegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.ToReadModel();
    }
}