using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MediaBrowser.Media;

public class HttpRequestExtensionsUnitTests
{
    [Test,
     TestCase(100, null, false),
     TestCase(100, "bytes=", false),
     TestCase(100, "bytes=0-", false),
     TestCase(100, "bytes=0-99", false),
     TestCase(100, "bytes=0-49", true),
     TestCase(100, "bytes=49-99", true),
     TestCase(100, "invalid", false),
     TestCase(100, "bytes=invalid-", false),
     TestCase(100, "bytes=invalid-invalid", false)]
    public void IsPartialRangeRequestTest(long length, string? rangeHeader, bool expected)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        if (rangeHeader != null)
        {
            request.Headers[HeaderNames.Range] = rangeHeader;
        }
        var result = request.IsPartialRangeRequest(length);
        result.ShouldBe(expected);
    }

    [Test,
     TestCase(null, "\"abc123\"", false),
     TestCase("\"other\"", "\"abc123\"", false),
     TestCase("\"abc123\"", "\"abc123\"", true)]
    public void DoEtagsMatchTests(string? requestEtag, string actualEtag, bool expected)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        if (requestEtag != null)
        {
            request.Headers[HeaderNames.IfNoneMatch] = requestEtag;
        }
        var result = request.DoEtagsMatch(actualEtag, 100);
        result.ShouldBe(expected);
    }

    [Test]
    public void DoEtagsMatchPartialRangeRequestReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var etag = "\"abc123\"";
        request.Headers[HeaderNames.IfNoneMatch] = etag;
        request.Headers[HeaderNames.Range] = "bytes=10-49";
        var result = request.DoEtagsMatch(etag, 50);
        result.ShouldBeFalse();
    }

    [Test,
     TestCase(null, "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("invalid-date", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:28:00 GMT", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:27:57 GMT", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:27:58 GMT", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:27:59 GMT", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:27:56 GMT", "Wed, 21 Oct 2015 07:28:00 GMT"),
     TestCase("Wed, 21 Oct 2015 07:28:01 GMT", "Wed, 21 Oct 2015 07:28:00 GMT")]
    public void WasModifiedSinceTests(string? requestLastModified, string actualLastModified)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        if (requestLastModified != null)
        {
            request.Headers[HeaderNames.IfModifiedSince] = requestLastModified;
        }
        var result = request.WasModifiedSince(DateTimeOffset.Parse(actualLastModified, CultureInfo.InvariantCulture), 100);
        result.ShouldBe(!string.Equals(requestLastModified, actualLastModified));
    }

    [Test]
    public void WasModifiedSincePartialRangeRequestReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.Range] = "bytes=10-49";
        var lastModified = DateTimeOffset.UtcNow;
        var result = request.WasModifiedSince(lastModified, 50);
        result.ShouldBeTrue();
    }
}