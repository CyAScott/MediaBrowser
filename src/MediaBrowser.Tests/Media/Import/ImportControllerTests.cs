using System.Net;
using System.Text.Json;
using System.Web;
using MediaBrowser.TestFiles;

// ReSharper disable AccessToDisposedClosure

namespace MediaBrowser.Media.Import;

public class ImportControllerTests
{
    [Test(Description = "Test CRUD Import APIs.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        await factory.StartServerAsync();

        var expectedFiles = await AddImportFiles(factory).ToDictionaryAsync(i => i.Name);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.GetJwtTokenForTestUser());

        await GetFilesTest();
        async Task GetFilesTest()
        {
            var actualFiles = await GetFiles(client);
            actualFiles.Count.ShouldBe(expectedFiles.Count);
            foreach (var actualFile in actualFiles)
            {
                actualFile.ShouldBe(expectedFiles[actualFile.Name]);
            }
        }

        await ReadFileNotFoundTest();
        async Task ReadFileNotFoundTest()
        {
            using var response = await ReadFile(client, "random-file-name");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        await ReadFileInfoNotFoundTest();
        async Task ReadFileInfoNotFoundTest()
        {
            using var response = await ReadFileInfo(client, "random-file-name");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        await ImportNotFoundTest();
        async Task ImportNotFoundTest()
        {
            using var response = await Import(client, "random-file-name", new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = [],
                Directors = [],
                Genres = [],
                Producers = [],
                Writers = []
            });
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        await ReadFileValidTest();
        async Task ReadFileValidTest()
        {
            var validFile = expectedFiles[_validFileName];
            using var response = await ReadFile(client, validFile.Name);
            response.EnsureSuccessStatusCode();
            response.Content.Headers.ContentLength.ShouldBe(validFile.Size);
            response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("video/mp4");
            var lastModifiedOn = response.Content.Headers.LastModified.ShouldNotBeNull().UtcDateTime;
            using var achedResponse = await ReadFile(client, validFile.Name, lastModifiedOn);
            achedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        }

        await ReadFileInfoValidTest();
        async Task ReadFileInfoValidTest()
        {
            var validFile = expectedFiles[_validFileName];
            using var response = await ReadFileInfo(client, validFile.Name);
            response.EnsureSuccessStatusCode();
            var actualFile = await response.Read<ImportFileInfo>();
            actualFile.ShouldBe(validFile);
        }

        await ImportInvalidTest();
        async Task ImportInvalidTest()
        {
            var invalidFile = expectedFiles[_invalidFileName];
            using var invalidCastResponse = await Import(client, invalidFile.Name, new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = ["_actor1_", "_actor2_"],
                Directors = [],
                Genres = [],
                Producers = [],
                Writers = []
            });
            invalidCastResponse.StatusCode.ShouldBe(HttpStatusCode.ExpectationFailed);
            using var invalidFileResponse = await Import(client, invalidFile.Name, new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = [],
                Directors = [],
                Genres = [],
                Producers = [],
                Writers = []
            });
            invalidFileResponse.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);
        }

        await ImportValidTest();
        async Task ImportValidTest()
        {
            var validFile = expectedFiles[_validFileName];

            using var invalidThumbnailResponse = await Import(client, validFile.Name, new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = ["actor"],
                Directors = ["director"],
                Genres = ["genre"],
                Producers = ["producer"],
                Writers = ["writer"],
                Thumbnail = 60 // 1 minute into the video which is valid for our 1-second test video
            });
            invalidThumbnailResponse.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);

            using var validFileResponse = await Import(client, validFile.Name, new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = ["actor"],
                Directors = ["director"],
                Genres = ["genre"],
                Producers = ["producer"],
                Writers = ["writer"],
                Thumbnail = 0.5
            });
            validFileResponse.EnsureSuccessStatusCode();

            var actual = await validFileResponse.Read<MediaReadModel>();
            actual.ShouldNotBeNull();

            using var scope = factory.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            var media = await db.MediaJoined.FirstOrDefaultAsync(m => m.Id == actual.Id);
            media.ShouldNotBeNull();

            var expected = media.ToReadModel(scope.ServiceProvider.GetRequiredService<MediaConfig>());
            actual.ShouldBe(expected);
        }
    }

    const string _invalidFileName = "invalid.mp4", _validFileName = "valid.mp4";
    async IAsyncEnumerable<ImportFileInfo> AddImportFiles(MediaBrowserWebApplicationFactory factory)
    {
        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();

        var invalidFilePath = Path.Combine(factory.ImportDirectory, _invalidFileName);
        await File.AppendAllTextAsync(invalidFilePath, "invalid content");
        yield return ImportFileInfo.Create(mediaConfig, invalidFilePath).ShouldNotBeNull();

        var validFilePath = Path.Combine(factory.ImportDirectory, _validFileName);
        await Files.Mp4(validFilePath);
        yield return ImportFileInfo.Create(mediaConfig, validFilePath).ShouldNotBeNull();
    }

    async Task<IReadOnlyList<ImportFileInfo>> GetFiles(HttpClient client)
    {
        using var response = await client.GetAsync("/api/import/files");
        response.EnsureSuccessStatusCode();
        return await response.Read<IReadOnlyList<ImportFileInfo>>();
    }
    async Task<HttpResponseMessage> ReadFile(HttpClient client, string name, DateTime? lastModified = null) =>
        await client.SendAsync(new(HttpMethod.Get, $"/api/import/file/{HttpUtility.UrlPathEncode(name)}")
        {
            Headers =
            {
                IfModifiedSince = lastModified
            }
        });
    Task<HttpResponseMessage> ReadFileInfo(HttpClient client, string name) =>
        client.GetAsync($"/api/import/file/{HttpUtility.UrlPathEncode(name)}/info");
    Task<HttpResponseMessage> Import(HttpClient client, string name, ImportMediaRequest request) =>
        client.PostAsync($"/api/import/file/{HttpUtility.UrlPathEncode(name)}",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
}