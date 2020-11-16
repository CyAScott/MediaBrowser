using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace MediaBrowser.Filters
{
    /// <summary>
    /// A service for managing JWTs.
    /// </summary>
    public class Jwt : IAuthorizationFilter
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class JwtClaimsPrincipal : ClaimsPrincipal
        {
            public JwtClaimsPrincipal(JwtPayload jwt)
                : base(jwt)
            {
                Identity = jwt;
            }

            public override IIdentity Identity { get; }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Cookies.TryGetValue(Config.CookieName, out var jwt) &&
                TryParseJwt(HttpUtility.UrlDecode(jwt.Replace("+", "%2B")), out var payload))
            {
                context.HttpContext.User = new JwtClaimsPrincipal(payload);
                return;
            }

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var allowAnonymousAttributes =
                (actionDescriptor?.MethodInfo?.GetCustomAttributes<AllowAnonymousAttribute>() ?? Enumerable.Empty<AllowAnonymousAttribute>()).Concat(
                actionDescriptor?.ControllerTypeInfo?.GetCustomAttributes<AllowAnonymousAttribute>() ?? Enumerable.Empty<AllowAnonymousAttribute>());

            if (allowAnonymousAttributes.Any())
            {
                return;
            }

            if (actionDescriptor?.ControllerTypeInfo?.GetCustomAttributes<ApiControllerAttribute>() == null)
            {
                context.Result = new RedirectResult($"/Login?returnUrl={HttpUtility.UrlEncode(context.HttpContext.Request.Path)}");
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }

        /// <inheritdoc cref="IJwtDecoder"/>
        protected IJwtDecoder Decoder { get; }

        /// <inheritdoc cref="IJwtEncoder"/>
        protected IJwtEncoder Encoder { get; }

        /// <inheritdoc/>
        public Jwt(AuthConfig config)
        {
            Config = config;

            var algorithm = new HMACSHA256Algorithm();
            var provider = new UtcDateTimeProvider();
            var urlEncoder = new JwtBase64UrlEncoder();
            var serializer = new JsonNetSerializer();

            var validator = new JwtValidator(serializer, provider);

            Decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            Encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        }

        /// <summary>
        /// Contains configuration settings for authentication and authorization.
        /// </summary>
        public AuthConfig Config { get; }

        /// <summary>
        /// Attempts to parse and validate a JWT string.
        /// </summary>
        public bool TryParseJwt(string jwt, out JwtPayload payload)
        {
            try
            {
                var json = Decoder.Decode(jwt);

                payload = new JwtPayload(JObject.Parse(json));

                return true;
            }
            catch
            {
                payload = null;

                return false;
            }
        }

        /// <summary>
        /// Attempts to parse and validate a JWT string.
        /// </summary>
        public string CreateJwt(JwtPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var json = payload.ToJson();

            return Encoder.Encode(json, Config.JwtSecret);
        }

        /// <summary>
        /// Adds a JWT cookie.
        /// </summary>
        public void SetJwtCookie(HttpContext context, JwtPayload payload)
        {
            payload.CreatedOn = DateTime.UtcNow;
            payload.ExpiresOn = payload.CreatedOn.Value.AddSeconds(Config.ExpirationInSeconds);
            context.Response.Cookies.Append(Config.CookieName, CreateJwt(payload), new CookieOptions
            {
                Domain = context.Request.Host.Value.Split(':').First(),
                Expires = payload.ExpiresOn,
                HttpOnly = true,
                Path = "/",
                Secure = context.Request.IsHttps
            });
        }

        /// <summary>
        /// Removes a JWT cookie.
        /// </summary>
        public void UnsetJwtCookie(HttpContext context) =>
            context.Response.Cookies.Append(Config.CookieName, "logout", new CookieOptions
            {
                Domain = context.Request.Host.Value.Split(':').First(),
                Expires = DateTime.UtcNow.AddYears(-1),
                HttpOnly = true,
                Path = "/",
                Secure = context.Request.IsHttps
            });
    }
}
