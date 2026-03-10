namespace MediaBrowser.Users;

public static class UserConfigExtensions
{
    extension(UserConfig userConfig)
    {
        public (string Jwt, DateTime Expires) GetJwt(Guid id, string username)
        {
            var expires = DateTime.UtcNow.AddMinutes(userConfig.JwtExpiryMinutes);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(userConfig.JwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new(
                [
                    new(ClaimTypes.Name, username),
                    new(ClaimTypes.NameIdentifier, id.ToString())
                ]),
                Expires = expires,
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = userConfig.JwtIssuer,
                Audience = userConfig.JwtAudience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), expires);
        }
    }
}