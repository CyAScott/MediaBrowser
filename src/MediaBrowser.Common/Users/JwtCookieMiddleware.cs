namespace MediaBrowser.Users;

public class JwtCookieMiddleware(RequestDelegate next)
{
    public const string CookieName = "mb_auth";

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CookieName, out var token))
        {
            context.Request.Headers.Authorization = $"Bearer {token}";
        }
        await next(context);
    }
}