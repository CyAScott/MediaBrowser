using Shouldly;

namespace MediaBrowser.Tests;

/// <summary>
/// Integration tests for <see cref="IndexController"/>.
/// </summary>
public class IndexPageTests
{
    [Test(Description = "Test that the index route catches all routes and responds with the index.html file.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        await factory.StartServerAsync();

        using var client = factory.CreateClient();

        using var indexPage = await client.GetAsync("/");
        indexPage.EnsureSuccessStatusCode();
        indexPage.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        var file = await indexPage.Content.ReadAsStringAsync();
        file.ShouldNotBeNullOrEmpty();

        using var indexPageWithRoute = await client.GetAsync("/test/route");
        indexPageWithRoute.EnsureSuccessStatusCode();
        indexPage.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        var fileWithRoute = await indexPage.Content.ReadAsStringAsync();

        fileWithRoute.ShouldBe(file);
    }
}