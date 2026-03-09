using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MediaBrowser.Media;

public class HttpRequestExtensionsUnitTests
{
    [Test]
    public void IsPartialRangeRequestNoRangeHeaderReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var result = request.IsPartialRangeRequest(100);
        result.ShouldBeFalse();
    }

    [Test]
    public void IsPartialRangeRequestValidPartialRangeReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.Range] = "bytes=10-49";
        var result = request.IsPartialRangeRequest(50);
        result.ShouldBeTrue();
    }

    [Test]
    public void IsPartialRangeRequestWholeFileRangeReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.Range] = "bytes=0-99";
        var result = request.IsPartialRangeRequest(100);
        result.ShouldBeFalse();
    }

    [Test]
    public void IsPartialRangeRequestInvalidRangeHeaderReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.Range] = "invalid";
        var result = request.IsPartialRangeRequest(100);
        result.ShouldBeFalse();
    }

    [Test]
    public void IsPartialRangeRequestInvalidRangeValuesHeaderReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.Range] = "bytes=invalid-invalid";
        var result = request.IsPartialRangeRequest(100);
        result.ShouldBeFalse();
    }

    [Test]
    public void DoEtagsMatchValidEtagAndNotPartialRangeReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var etag = "\"abc123\"";
        request.Headers[HeaderNames.IfNoneMatch] = etag;
        var result = request.DoEtagsMatch(etag, 100);
        result.ShouldBeTrue();
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

    [Test]
    public void DoEtagsMatchMismatchedEtagReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.IfNoneMatch] = "\"other\"";
        var result = request.DoEtagsMatch("\"abc123\"", 100);
        result.ShouldBeFalse();
    }

    [Test]
    public void WasModifiedSinceNoIfModifiedSinceHeaderReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var lastModified = DateTimeOffset.UtcNow;
        var result = request.WasModifiedSince(lastModified, 100);
        result.ShouldBeTrue();
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

    [Test]
    public void WasModifiedSinceIfModifiedSinceMatchesReturnsFalse()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var lastModified = DateTimeOffset.UtcNow;
        request.Headers[HeaderNames.IfModifiedSince] = lastModified.ToString("R", CultureInfo.InvariantCulture);
        var result = request.WasModifiedSince(lastModified, 100);
        result.ShouldBeFalse();
    }

    [Test]
    public void WasModifiedSinceIfModifiedSinceDiffersByMoreThan2SecondsReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var lastModified = DateTimeOffset.UtcNow;
        var oldDate = lastModified.AddSeconds(-10);
        request.Headers[HeaderNames.IfModifiedSince] = oldDate.ToString("R", CultureInfo.InvariantCulture);
        var result = request.WasModifiedSince(lastModified, 100);
        result.ShouldBeTrue();
    }

    [Test]
    public void WasModifiedSinceInvalidIfModifiedSinceHeaderReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HeaderNames.IfModifiedSince] = "not-a-date";
        var lastModified = DateTimeOffset.UtcNow;
        var result = request.WasModifiedSince(lastModified, 100);
        result.ShouldBeTrue();
    }
}