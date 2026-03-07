namespace MediaBrowser.Media;

public static class HttpRequestExtensions
{
    public static bool WasModifiedSince(this HttpRequest request, DateTime lastModified) =>
        !request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceValue)
        || !DateTime.TryParse(ifModifiedSinceValue, CultureInfo.InvariantCulture, out var ifModifiedSince)
        || Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) > 2;
}