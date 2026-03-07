namespace MediaBrowser;

public class SwaggerTest
{
    [Test(Description = "Test that the swagger pages work.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        await factory.StartServerAsync();

        using var client = factory.CreateClient();

        using var swaggerPage = await client.GetAsync("/swagger");
        swaggerPage.EnsureSuccessStatusCode();

        using var swaggerJsonFile = await client.GetAsync($"/swagger/{Installer.Version}/swagger.json");
        swaggerJsonFile.EnsureSuccessStatusCode();
    }
}