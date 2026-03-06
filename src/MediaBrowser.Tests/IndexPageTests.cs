using Shouldly;

namespace MediaBrowser.Tests;

public class IndexPageTests
{
    [Test(Description = "Test that the index route catches all routes and loads the index.html page.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        factory.StartServer();

        using var client = factory.CreateClient();

        using var indexPage = await client.GetAsync("/");
        indexPage.EnsureSuccessStatusCode();
        indexPage.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        var file = await indexPage.Content.ReadAsStringAsync();

        using var indexPageWithRoute = await client.GetAsync("/test/route");
        indexPageWithRoute.EnsureSuccessStatusCode();
        indexPage.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        var fileWithRoute = await indexPage.Content.ReadAsStringAsync();

        fileWithRoute.ShouldBe(file);
    }
}