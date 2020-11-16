using MediaBrowser.Attributes;
using System;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Contains configuration settings for authentication and authorization.
    /// </summary>
    [Configuration("Auth")]
    public class AuthConfig
    {
        /// <summary>
        /// The cookie name for storing the JWT.
        /// </summary>
        public string CookieName { get; set; }

        /// <summary>
        /// The secret for signing JWTs.
        /// </summary>
        public string JwtSecret { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The initial user first name that gets created automatically.
        /// </summary>
        public string InitFirstName { get; set; } = "Admin";

        /// <summary>
        /// The initial user last name that gets created automatically.
        /// </summary>
        public string InitLastName { get; set; } = "User";

        /// <summary>
        /// The initial user name that gets created automatically.
        /// </summary>
        public string InitUserName { get; set; } = "admin";

        /// <summary>
        /// The initial user password that gets created automatically.
        /// </summary>
        public string InitUserPassword { get; set; } = "admin";

        /// <summary>
        /// The JWT expiration in seconds.
        /// </summary>
        public int ExpirationInSeconds { get; set; }
    }
}
