using JWT;
using MediaBrowser.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace MediaBrowser
{
    /// <summary>
    /// The data stored inside a JWT.
    /// </summary>
    public class JwtPayload : IIdentity, IUser
    {
        DateTime? IUser.DeletedOn => null;
        bool IUser.IsPasswordValid(string password) => false;
        string[] IUser.Roles => Roles.ToArray();

        /// <inheritdoc/>
        public JwtPayload(JObject obj = null)
        {
            if (obj == null)
            {
                return;
            }

            CreatedOn = obj.TryGetValue("iat", out var createdOn) && createdOn.Type == JTokenType.Integer ? (DateTime?)UnixEpoch.Value.AddSeconds(createdOn.Value<long>()) : null;
            ExpiresOn = obj.TryGetValue("exp", out var expiresOn) && expiresOn.Type == JTokenType.Integer ? (DateTime?)UnixEpoch.Value.AddSeconds(expiresOn.Value<long>()) : null;
            FirstName = obj.TryGetValue("fName", out var firstName) && firstName.Type == JTokenType.String ? firstName.Value<string>() : null;
            Id = obj.TryGetValue("sub", out var id) && id.Type == JTokenType.String ? new Guid(id.Value<string>()) : Guid.Empty;
            LastName = obj.TryGetValue("lName", out var lastName) && lastName.Type == JTokenType.String ? lastName.Value<string>() : null;
            UserName = obj.TryGetValue("uName", out var userName) && userName.Type == JTokenType.String ? userName.Value<string>() : null;

            if (obj.TryGetValue("roles", out var roles) && roles.Type == JTokenType.Array)
            {
                foreach (var role in ((JArray)roles)
                    .Where(it => it.Type == JTokenType.String)
                    .Select(it => it.Value<string>()))
                {
                    Roles.Add(role);
                }
            }
        }

        /// <summary>
        /// Serializes the payload to a <see cref="JObject"/>.
        /// </summary>
        public JObject ToJson() => new JObject
        {
            { "exp", ExpiresOn == null ? 0L : UnixEpoch.GetSecondsSince(ExpiresOn.Value) },
            { "fName", FirstName ?? "" },
            { "roles", new JArray(Roles.Cast<object>().ToArray()) },
            { "iat", CreatedOn == null ? 0L : UnixEpoch.GetSecondsSince(CreatedOn.Value) },
            { "lName", LastName ?? "" },
            { "sub", Id.ToString() },
            { "uName", UserName ?? "" }
        };

        /// <summary>
        /// When the JWT was created.
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// When the JWT is set to expire.
        /// </summary>
        public DateTime? ExpiresOn { get; set; }

        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <summary>
        /// The role names the user was assigned to.
        /// </summary>
        public HashSet<string> Roles { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool IsAuthenticated => true;

        /// <inheritdoc/>
        public string AuthenticationType => "JWT";

        /// <inheritdoc/>
        public string FirstName { get; set; }

        /// <inheritdoc/>
        public string LastName { get; set; }

        /// <inheritdoc/>
        public string Name => $"{FirstName} {LastName}";

        /// <inheritdoc/>
        public string UserName { get; set; }
    }
}
