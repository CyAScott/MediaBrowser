using Microsoft.Net.Http.Headers;

namespace MediaBrowser.Media;

public static class HttpRequestExtensions
{
    extension(HttpRequest request)
    {
        public bool IsPartialRangeRequest(long fileLength)
        {
            if (!request.Headers.TryGetValue(HeaderNames.Range, out var rangeHeader) || rangeHeader.Count == 0)
            {
                return false;
            }

            var range = rangeHeader[0];
            if (string.IsNullOrEmpty(range) || !range.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var parts = range[6..].Split('-', 2);
            if (parts.Length != 2)
            {
                return false;
            }
            if (!long.TryParse(parts[0], CultureInfo.InvariantCulture, out var start))
            {
                start = 0;
            }

            if (!long.TryParse(parts[1], CultureInfo.InvariantCulture, out var end))
            {
                end = fileLength - 1;
            }
            // If the range covers the whole file, it's not partial
            return !(start == 0 && end == fileLength - 1);
        }

        public bool DoEtagsMatch(string etag, long fileLength) =>
            // If it's a range request, we should ignore the ETag and treat it as modified to allow for proper range handling
            !request.IsPartialRangeRequest(fileLength)
            && request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatchValue)
            && ifNoneMatchValue.Count == 1
            && string.Equals(ifNoneMatchValue[0], etag, StringComparison.OrdinalIgnoreCase);

        public bool WasModifiedSince(DateTimeOffset lastModified, long fileLength) =>
            // If it's a range request, we should ignore the If-Modified-Since header and treat it as modified to allow for proper range handling
            request.IsPartialRangeRequest(fileLength)
            || !request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var ifModifiedSinceValue)
            || ifModifiedSinceValue.Count != 1
            || !DateTimeOffset.TryParse(ifModifiedSinceValue[0], CultureInfo.InvariantCulture, out var ifModifiedSince)
            || Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) > 2;
    }

}