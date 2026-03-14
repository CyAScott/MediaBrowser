

// ReSharper disable AccessToDisposedClosure

namespace MediaBrowser.Media.Import;

public class ImportControllerTests
{
    [Test(Description = "Test CRUD import APIs.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        await factory.StartServerAsync();

        var (validFile, invalidFile) = await AddImportFiles(factory);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.GetJwtForTestUser());
        var importClient = new ImportClient(client);

        await GetFilesTest();
        async Task GetFilesTest()
        {
            var testFiles = new[]
                {
                    invalidFile,
                    validFile
                }
                .OrderBy(it => it.Name)
                .ToList();
            using var response = await importClient.GetFiles();
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull()
                .Count.ShouldBe(testFiles.Count, "The number of files returned should match the number of files added for this test.");
            foreach (var (actualFile, expectedFile) in response.Content
                .OrderBy(it => it.Name)
                .Zip(testFiles))
            {
                actualFile.ShouldBe(expectedFile);
            }
        }

        await ReadFileNotFoundTest();
        async Task ReadFileNotFoundTest()
        {
            using var response = await importClient.ReadFile("random-file-name");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The file does not exist, so it should return 404 Not Found.");
        }

        await ReadFileInfoNotFoundTest();
        async Task ReadFileInfoNotFoundTest()
        {
            using var response = await importClient.ReadFileInfo("random-file-name");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The file does not exist, so it should return 404 Not Found.");
        }

        await ImportNotFoundTest();
        async Task ImportNotFoundTest()
        {
            using var response = await importClient.Import("random-file-name", new()
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
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The file does not exist, so it should return 404 Not Found.");
        }

        await ReadFileValidTest();
        async Task ReadFileValidTest()
        {
            // Read the file test file and ensure the content and headers are correct.
            // This also serves as a baseline for the subsequent test to ensure that the file is cached after the first read.
            using var response = await importClient.ReadFile(validFile.Name);
            response.EnsureSuccessStatusCode();
            response.Content.Headers.ContentLength.ShouldBe(validFile.Size);
            response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe(validFile.Mime);

            // Read the file again with the Last-Modified header to ensure it returns 304 Not Modified since the file has not changed.
            var lastModifiedOn = response.Content.Headers.LastModified.ShouldNotBeNull();
            using var cachedResponse = await importClient.ReadFile(validFile.Name, lastModifiedOn);
            cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        }

        await ReadFileInfoValidTest();
        async Task ReadFileInfoValidTest()
        {
            using var response = await importClient.ReadFileInfo(validFile.Name);
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldBe(validFile);
        }

        await ImportInvalidTest();
        async Task ImportInvalidTest()
        {
            using var response = await importClient.Import(invalidFile.Name, new()
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
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The file is not a valid media file, so it should return 406 Not Acceptable.");
        }

        await InvalidTagTest(TagType.Cast);
        await InvalidTagTest(TagType.Director);
        await InvalidTagTest(TagType.Genre);
        await InvalidTagTest(TagType.Producer);
        await InvalidTagTest(TagType.Writer);
        async Task InvalidTagTest(TagType tagType)
        {
            // The tag must be alphanumeric characters only,
            // so using an invalid tag with underscores should cause the import to fail with 417 Expectation Failed.
            const string invalidTag = "_tag_";

            using var response = await importClient.Import(validFile.Name, new()
            {
                Title = "title",
                OriginalTitle = "original title",
                Description = "description",
                Cast = tagType == TagType.Cast ? [invalidTag] : [],
                Directors = tagType == TagType.Director ? [invalidTag] : [],
                Genres = tagType == TagType.Genre ? [invalidTag] : [],
                Producers = tagType == TagType.Producer ? [invalidTag] : [],
                Writers = tagType == TagType.Writer ? [invalidTag] : []
            });
            response.StatusCode.ShouldBe(HttpStatusCode.ExpectationFailed,
                "Only alphanumeric characters are allowed for cast, directors, genres, producers, and writers, so it should return 417 Expectation Failed.");
        }

        await ImportValidFileWithInvalidThumbnailTest();
        async Task ImportValidFileWithInvalidThumbnailTest()
        {
            using var response = await importClient.Import(validFile.Name, new()
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
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The thumbnail timestamp is beyond the duration of the video, so it should return 406 Not Acceptable.");
        }

        await ImportValidTest();
        async Task ImportValidTest()
        {
            using var response = await importClient.Import(validFile.Name, new()
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
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull();

            using var scope = factory.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            var media = await db.MediaJoined.FirstOrDefaultAsync(m => m.Id == response.Content.Id);
            media.ShouldNotBeNull();

            var expected = media.ToReadModel(scope.ServiceProvider.GetRequiredService<MediaConfig>());
            response.Content.ShouldBe(expected);
        }

        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();
        await AddFileTests(mediaConfig, importClient);
    }

    async Task AddFileTests(MediaConfig mediaConfig, ImportClient importClient)
    {
        const string validFileName = "valid.mp4";

        await ImportDirectoryMissingTest();
        async Task ImportDirectoryMissingTest()
        {
            Directory.Delete(mediaConfig.ImportDirectory!, true);
            using var response = await importClient.Add(new MemoryStream(Guid.NewGuid().ToByteArray()), validFileName);
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The import directory is missing, so it should return 406 Not Acceptable Error.");
            Directory.CreateDirectory(mediaConfig.ImportDirectory!);
        }

        var invalidFileNames = new[]
        {
            "invalid",
            ".invalid.mp4",
            "invalid.txt",
            "invalid/../file.mp4",
            @"invalid\..\file.mp4",
            "invalid:file.mp4",
            "invalid*file.mp4",
            "invalid?file.mp4",
            "invalid<file.mp4",
            "invalid>file.mp4",
            "invalid|file.mp4"
        };
        foreach (var invalidFileName in invalidFileNames)
        {
            await InvalidFilesNamesTest(invalidFileName);
        }
        async Task InvalidFilesNamesTest(string fileName)
        {
            using var response = await importClient.Add(new MemoryStream(Guid.NewGuid().ToByteArray()), fileName);
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The file name is invalid, so it should return 406 Not Acceptable Error.");
        }

        await DuplicateFileTest();
        async Task DuplicateFileTest()
        {
            const string existingFileName = "existing.mp4";
            var validFilePath = Path.Combine(mediaConfig.ImportDirectory!, existingFileName);
            await File.WriteAllTextAsync(validFilePath, Guid.NewGuid().ToString());

            using var response = await importClient.Add(new MemoryStream(Guid.NewGuid().ToByteArray()), existingFileName);
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict,
                "The file already exists, so it should return 409 Conflict.");
        }

        await ValidFileTest();
        async Task ValidFileTest()
        {
            var contents = Guid.NewGuid().ToByteArray();
            using var response = await importClient.Add(new MemoryStream(contents), validFileName);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, "The file should have been added successfully, so it should return 200 OK.");
            var expectedPath = Path.Combine(mediaConfig.ImportDirectory!, validFileName);
            File.Exists(expectedPath).ShouldBeTrue("The file should have been added successfully, so it should return 200 OK.");
            (await File.ReadAllBytesAsync(expectedPath)).ShouldBe(contents, "The contents of the file should match the contents that were uploaded.");
        }
    }

    async Task<(ImportFileInfo ValidFile, ImportFileInfo InvalidFile)> AddImportFiles(MediaBrowserWebApplicationFactory factory)
    {
        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();

        var invalidFilePath = Path.Combine(factory.ImportDirectory, "invalid.mp4");
        await File.AppendAllTextAsync(invalidFilePath, "invalid content");
        var invalidFile = ImportFileInfo.Create(mediaConfig, invalidFilePath).ShouldNotBeNull();

        var validFilePath = Path.Combine(factory.ImportDirectory, "valid.mp4");
        await Files.Mp4(validFilePath);
        var validFile = ImportFileInfo.Create(mediaConfig, validFilePath).ShouldNotBeNull();

        return (validFile, invalidFile);
    }
}