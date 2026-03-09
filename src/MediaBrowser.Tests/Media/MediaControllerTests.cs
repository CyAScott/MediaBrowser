using System.Net;
using MediaBrowser.Media.Import;

namespace MediaBrowser.Media;

public class MediaControllerTests
{
    [Test(Description = "Test CRUD media APIs.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        await factory.StartServerAsync();

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", factory.GetJwtForTestUser());
        var mediaClient = new MediaClient(client);

        var testImageFile = await AddTestImageFile(factory, client);
        var testVideoFile = await AddTestVideoFile(factory, client);

        await GetNotFoundTest();
        async Task GetNotFoundTest()
        {
            using var response = await mediaClient.Get(Guid.NewGuid());
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The media does not exist because this is a random media ID, so it should return 404 Not Found.");
        }

        await GetFoundTest();
        async Task GetFoundTest()
        {
            using var response = await mediaClient.Get(testVideoFile.Id);
            const string message = "This should return the media details successfully since we are using a valid media ID.";
            response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.ShouldBe(testVideoFile, message);
        }

        await SearchTests();
        async Task SearchTests()
        {
            using var response = await mediaClient.Search(new()
            {
                Take = 100
            });
            const string message = "This should return search results successfully since we have added media files, and we are not applying any filters.";
            response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.Count.ShouldBe(2, message);
            response.Content.Results.ShouldContain(testImageFile, message);
            response.Content.Results.ShouldContain(testVideoFile, message);
        }

        await SearchKeywordTests();
        async Task SearchKeywordTests()
        {
            using var response = await mediaClient.Search(new()
            {
                Keywords = testImageFile.Title,
                Take = 100
            });
            const string message = "This should return search results successfully since we have added media files, and we are not applying any filters.";
            response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.Count.ShouldBe(1, message);
            response.Content.Results.ShouldContain(testImageFile, message);
        }

        await StreamThumbnailTest(testImageFile);
        await StreamThumbnailTest(testVideoFile);
        async Task StreamThumbnailTest(MediaReadModel testFile)
        {
             using var response = await mediaClient.StreamThumbnail(testFile.Id);
             const string message = "This should return the thumbnail successfully since we are using a valid media ID.";
             response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
             response.Content.ShouldNotBeNull(message);
             response.Content.Headers.ContentLength.ShouldNotBeNull(message).ShouldNotBe(0, message);
             response.Content.Headers.ContentType.ShouldNotBeNull(message).MediaType
                 .ShouldBe(testFile.Mime.StartsWith("video/") ? "image/jpeg" : testFile.Mime, message);

             // Read the file again with the Last-Modified header to ensure it returns 304 Not Modified since the file has not changed.
             const string cachedMessage = "This should return 304 Not Modified since the file has not changed.";
             var lastModifiedOn = response.Content.Headers.LastModified.ShouldNotBeNull(cachedMessage);
             using var cachedResponse = await mediaClient.StreamThumbnail(testFile.Id, lastModifiedOn);
             cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified, cachedMessage);
        }

        await StreamFanartThumbnailTest(testImageFile);
        await StreamFanartThumbnailTest(testVideoFile);
        async Task StreamFanartThumbnailTest(MediaReadModel testFile)
        {
             using var response = await mediaClient.StreamFanartThumbnail(testFile.Id);
             const string message = "This should return the thumbnail successfully since we are using a valid media ID.";
             response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
             response.Content.ShouldNotBeNull(message);
             response.Content.Headers.ContentLength.ShouldNotBeNull(message).ShouldNotBe(0, message);
             response.Content.Headers.ContentType.ShouldNotBeNull(message).MediaType
                 .ShouldBe(testFile.Mime.StartsWith("video/") ? "image/jpeg" : testFile.Mime, message);

             // Read the file again with the Last-Modified header to ensure it returns 304 Not Modified since the file has not changed.
             const string cachedMessage = "This should return 304 Not Modified since the file has not changed.";
             var lastModifiedOn = response.Content.Headers.LastModified.ShouldNotBeNull(cachedMessage);
             using var cachedResponse = await mediaClient.StreamFanartThumbnail(testFile.Id, lastModifiedOn);
             cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified, cachedMessage);
         }

        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();
        await StreamTests(mediaConfig, mediaClient, testImageFile);
        await StreamTests(mediaConfig, mediaClient, testVideoFile);

        await TagTests(mediaClient);

        await UpdateNotFoundTest();
        async Task UpdateNotFoundTest()
        {
            using var response = await mediaClient.Update(Guid.NewGuid(), new()
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
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "This should return 404 Not Found since we are using a random media ID that does not exist.");
        }

        await UpdateInvalidTagTest(TagType.Cast);
        await UpdateInvalidTagTest(TagType.Director);
        await UpdateInvalidTagTest(TagType.Genre);
        await UpdateInvalidTagTest(TagType.Producer);
        await UpdateInvalidTagTest(TagType.Writer);
        async Task UpdateInvalidTagTest(TagType tagType)
        {
            // The tag must be alphanumeric characters only,
            // so using an invalid tag with underscores should cause the import to fail with 417 Expectation Failed.
            const string invalidTag = "_tag_";

            using var response = await mediaClient.Update(testVideoFile.Id, new()
            {
                Title = testVideoFile.Title,
                OriginalTitle = testVideoFile.OriginalTitle,
                Description = testVideoFile.Description,
                Cast = tagType == TagType.Cast ? [invalidTag] : [],
                Directors = tagType == TagType.Director ? [invalidTag] : [],
                Genres = tagType == TagType.Genre ? [invalidTag] : [],
                Producers = tagType == TagType.Producer ? [invalidTag] : [],
                Writers = tagType == TagType.Writer ? [invalidTag] : []
            });
            response.StatusCode.ShouldBe(HttpStatusCode.ExpectationFailed,
                "Only alphanumeric characters are allowed for cast, directors, genres, producers, and writers, so it should return 417 Expectation Failed.");
        }

        await UpdateSuccessTest();
        async Task UpdateSuccessTest()
        {
            using var response = await mediaClient.Update(testVideoFile.Id, new()
            {
                Title = "new title",
                OriginalTitle = "new original title",
                Description = "new description",
                Cast = ["new cast"],
                Directors = ["new director"],
                Genres = ["new genre"],
                Producers = ["new producer"],
                Writers = ["new writer"]
            });
            const string message = "This should update the media details successfully since we are using a valid media ID and valid details.";
            response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.Title.ShouldBe("new title", message);
            response.Content.OriginalTitle.ShouldBe("new original title", message);
            response.Content.Description.ShouldBe("new description", message);
            response.Content.Cast.ShouldBe(["new cast"], message);
            response.Content.Directors.ShouldBe(["new director"], message);
            response.Content.Genres.ShouldBe(["new genre"], message);
            response.Content.Producers.ShouldBe(["new producer"], message);
            response.Content.Writers.ShouldBe(["new writer"], message);
        }

        await UpdateThumbnailWithFileTests(mediaConfig, mediaClient, testVideoFile, testImageFile);
        await UpdateThumbnailWithTimestampTests(mediaConfig, mediaClient, testVideoFile, testImageFile);
    }

    async Task StreamTests(MediaConfig mediaConfig, MediaClient mediaClient, MediaReadModel testFile)
    {
        await MediaNotFoundTest();
        async Task MediaNotFoundTest()
        {
            using var response = await mediaClient.Stream(Guid.NewGuid());
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The media does not exist because this is a random media ID, so it should return 404 Not Found.");
        }

        await FileNotFoundTest();
        async Task FileNotFoundTest()
        {
            // To test the case where the media file is not found, we can temporarily rename the file so that it cannot be found, and then restore it after the test.
            var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{testFile.Md5}.{mediaConfig.GetExtensionFromMime(testFile.Mime)}");
            var tempFilePath = filePath + ".bak";
            File.Move(filePath, tempFilePath);

            using var response = await mediaClient.Stream(testFile.Id);
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The file does not exist because we have renamed it, so it should return 404 Not Found.");

            File.Move(tempFilePath, filePath);
        }

        var (lastModifiedOn, etag) = await ValidFileTest();
        async Task<(DateTimeOffset LastModifiedOn, string Etag)> ValidFileTest()
        {
            using var response = await mediaClient.Stream(testFile.Id);
            const string message = "This should return the file successfully since we are using a valid media ID.";
            response.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.Headers.ContentLength.ShouldNotBeNull(message).ShouldNotBe(0, message);
            response.Content.Headers.ContentType.ShouldNotBeNull(message).MediaType.ShouldBe(testFile.Mime, message);
            var lastModifiedOnValue = response.Content.Headers.LastModified.ShouldNotBeNull(message);
            var etagValue = response.Headers.ETag.ShouldNotBeNull(message).Tag;
            return (lastModifiedOnValue, etagValue);
        }

        await EtagTest();
        async Task EtagTest()
        {
            using var cachedResponse = await mediaClient.Stream(testFile.Id, etag: etag);
            cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified, "The file has not changed since we are using the ETag from the previous response, so it should return 304 Not Modified.");
        }

        await LastModifiedTest();
        async Task LastModifiedTest()
        {
            using var cachedResponse = await mediaClient.Stream(testFile.Id, lastModified: lastModifiedOn);
            cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified, "The file has not changed since we are using the Last-Modified timestamp from the previous response, so it should return 304 Not Modified.");
        }

        await RangeRequestTest();
        async Task RangeRequestTest()
        {
            using var response = await mediaClient.Stream(testFile.Id, rangeHeaderValue: new(1, 10));
            const string message = "This should return the file successfully since we are using a valid media ID and a valid range header.";
            response.StatusCode.ShouldBe(HttpStatusCode.PartialContent, message);
            response.Content.ShouldNotBeNull(message);
            response.Content.Headers.ContentLength.ShouldNotBeNull(message).ShouldBe(10, message);
            response.Content.Headers.ContentType.ShouldNotBeNull(message).MediaType.ShouldBe(testFile.Mime, message);
        }
    }

    async Task TagTests(MediaClient mediaClient)
    {
        await GetAllTest(TagType.Cast);
        await GetAllTest(TagType.Director);
        await GetAllTest(TagType.Genre);
        await GetAllTest(TagType.Producer);
        await GetAllTest(TagType.Writer);
        async Task GetAllTest(TagType tagType)
        {
            using var searchResponse = await mediaClient.Search(new()
            {
                Take = 100
            });
            searchResponse.EnsureSuccessStatusCode();
            searchResponse.Content.ShouldNotBeNull();

            var tags = tagType switch
            {
                TagType.Cast => searchResponse.Content.Results.SelectMany(m => m.Cast).Distinct().OrderBy(m => m).ToList(),
                TagType.Director => searchResponse.Content.Results.SelectMany(m => m.Directors).Distinct().OrderBy(m => m).ToList(),
                TagType.Genre => searchResponse.Content.Results.SelectMany(m => m.Genres).Distinct().OrderBy(m => m).ToList(),
                TagType.Producer => searchResponse.Content.Results.SelectMany(m => m.Producers).Distinct().OrderBy(m => m).ToList(),
                _ => searchResponse.Content.Results.SelectMany(m => m.Writers).Distinct().OrderBy(m => m).ToList()
            };

            using var allResponse = await mediaClient.GetAll(tagType);
            const string message = "This should return all tags successfully since we are using a valid tag type.";
            allResponse.StatusCode.ShouldBe(HttpStatusCode.OK, message);
            allResponse.Content.ShouldNotBeNull(message);
            allResponse.Content.ShouldBe(tags, message);
        }

        await GetThumbnailTest(TagType.Cast);
        await GetThumbnailTest(TagType.Director);
        await GetThumbnailTest(TagType.Genre);
        await GetThumbnailTest(TagType.Producer);
        await GetThumbnailTest(TagType.Writer);
        async Task GetThumbnailTest(TagType tagType)
        {
            using var searchResponse = await mediaClient.Search(new()
            {
                Take = 100
            });
            searchResponse.EnsureSuccessStatusCode();
            searchResponse.Content.ShouldNotBeNull();
            var tags = tagType switch
            {
                TagType.Cast => searchResponse.Content.Results.SelectMany(m => m.Cast).Distinct().OrderBy(m => m).ToList(),
                TagType.Director => searchResponse.Content.Results.SelectMany(m => m.Directors).Distinct().OrderBy(m => m).ToList(),
                TagType.Genre => searchResponse.Content.Results.SelectMany(m => m.Genres).Distinct().OrderBy(m => m).ToList(),
                TagType.Producer => searchResponse.Content.Results.SelectMany(m => m.Producers).Distinct().OrderBy(m => m).ToList(),
                _ => searchResponse.Content.Results.SelectMany(m => m.Writers).Distinct().OrderBy(m => m).ToList()
            };

            foreach (var tag in tags)
            {
                using var thumbnailResponse = await mediaClient.GetThumbnail(tagType, tag);
                if (tag.Contains(_noThumbnailTag))
                {
                    thumbnailResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The thumbnail should not exist.");
                }
                else
                {
                    const string message = "This should return the thumbnail successfully since we are using a valid tag that has a thumbnail.";
                    thumbnailResponse.StatusCode.ShouldBe(HttpStatusCode.OK, message);
                    thumbnailResponse.Content.ShouldNotBeNull(message);
                    thumbnailResponse.Content.Headers.ContentLength.ShouldNotBeNull(message);
                    thumbnailResponse.Content.Headers.ContentType.ShouldNotBeNull(message).MediaType.ShouldBe("image/jpeg", message);

                    var lastModifiedOn = thumbnailResponse.Content.Headers.LastModified.ShouldNotBeNull(message);
                    using var cachedResponse = await mediaClient.GetThumbnail(tagType, tag, lastModifiedOn);
                    cachedResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified, "The file has not changed since we are using the Last-Modified timestamp from the previous response, so it should return 304 Not Modified.");
                }
            }
        }
    }

    async Task UpdateThumbnailWithFileTests(MediaConfig mediaConfig, MediaClient mediaClient, MediaReadModel testVideoFile, MediaReadModel testImageFile)
    {
        var thumbnail = new FileInfo(Path.Combine(mediaConfig.ImportDirectory!, "valid-thumbnail.webp"));
        await TestFiles.Files.Image(thumbnail.FullName);

        await NotFoundTest();
        async Task NotFoundTest()
        {
            using var stream = thumbnail.OpenRead();
            using var response = await mediaClient.UpdateThumbnailWithFile(Guid.NewGuid(), stream, thumbnail.Name, isPrimary: true);
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The media does not exist because this is a random media ID, so it should return 404 Not Found.");
        }

        await ThumbnailForImageTest();
        async Task ThumbnailForImageTest()
        {
            using var stream = thumbnail.OpenRead();
            using var response = await mediaClient.UpdateThumbnailWithFile(testImageFile.Id, stream, thumbnail.Name, isPrimary: true);
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The media is an image file which does not support thumbnail update with an image file, so it should return 406 Not Acceptable.");
        }

        await InvalidThumbnailTest();
        async Task InvalidThumbnailTest()
        {
            var invalidThumbnail = new FileInfo(Path.Combine(mediaConfig.ImportDirectory!, "invalid-thumbnail.webp"));
            await File.WriteAllTextAsync(invalidThumbnail.FullName, "this is not a valid image file");
            using var stream = invalidThumbnail.OpenRead();
            using var response = await mediaClient.UpdateThumbnailWithFile(testVideoFile.Id, stream, invalidThumbnail.Name, isPrimary: true);
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable, "This should return 406 Not Acceptable since the file is not a valid image file.");
        }

        await ValidPrimaryThumbnailTest();
        async Task ValidPrimaryThumbnailTest()
        {
            using var stream = thumbnail.OpenRead();
            using var response = await mediaClient.UpdateThumbnailWithFile(testVideoFile.Id, stream, thumbnail.Name, isPrimary: true);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, "This should update the primary thumbnail successfully since we are using a valid media ID and a valid image file, so it should return 200 Ok.");
        }
        await ValidFanartThumbnailTest();
        async Task ValidFanartThumbnailTest()
        {
            using var stream = thumbnail.OpenRead();
            using var response = await mediaClient.UpdateThumbnailWithFile(testVideoFile.Id, stream, thumbnail.Name, isPrimary: false);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, "This should update the fanart thumbnail successfully since we are using a valid media ID and a valid image file, so it should return 200 Ok.");
        }
    }

    async Task UpdateThumbnailWithTimestampTests(MediaConfig mediaConfig, MediaClient mediaClient, MediaReadModel testVideoFile, MediaReadModel testImageFile)
    {
        await MediaNotFoundTest();
        async Task MediaNotFoundTest()
        {
            using var response = await mediaClient.UpdateThumbnailWithTimestamp(Guid.NewGuid(), new()
            {
                At = 0
            });
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The media does not exist because this is a random media ID, so it should return 404 Not Found.");
        }

        await ThumbnailForImageTest();
        async Task ThumbnailForImageTest()
        {
            using var response = await mediaClient.UpdateThumbnailWithTimestamp(testImageFile.Id, new()
            {
                At = 0
            });
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The media is an image file which does not support thumbnail update with an image file, so it should return 406 Not Acceptable.");
        }

        await FileNotFoundTest();
        async Task FileNotFoundTest()
        {
            // To test the case where the media file is not found,
            // we can temporarily rename the file so that it cannot be found,
            // and then restore it after the test.
            var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{testVideoFile.Md5}.{mediaConfig.GetExtensionFromMime(testVideoFile.Mime)}");
            var tempFilePath = $"{filePath}.bak";
            File.Move(filePath, tempFilePath);

            using var response = await mediaClient.UpdateThumbnailWithTimestamp(testVideoFile.Id, new()
            {
                At = 0
            });
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound, "The file does not exist because we have renamed it, so it should return 404 Not Found.");

            // Restore the file after the test.
            File.Move(tempFilePath, filePath);
        }

        await InvalidTimestampTest();
        async Task InvalidTimestampTest()
        {
            using var response = await mediaClient.UpdateThumbnailWithTimestamp(testVideoFile.Id, new()
            {
                At = 60 // 1 minute into the video which is valid for our 1-second test video, so it should return 400 Bad Request.
            });
            response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable,
                "The thumbnail timestamp is beyond the duration of the video, so it should return 406 Not Acceptable.");
        }

        await ValidPrimaryThumbnailTest();
        async Task ValidPrimaryThumbnailTest()
        {
            using var response = await mediaClient.UpdateThumbnailWithTimestamp(testVideoFile.Id, new()
            {
                At = 0.5
            });
            response.StatusCode.ShouldBe(HttpStatusCode.OK, "The thumbnail should be updated successfully since we are using a valid timestamp, so it should return 204 Ok.");
        }

        await ValidFanartThumbnailTest();
        async Task ValidFanartThumbnailTest()
        {
            using var response = await mediaClient.UpdateFanartThumbnailWithTimestamp(testVideoFile.Id, new()
            {
                At = 0.5
            });
            response.StatusCode.ShouldBe(HttpStatusCode.OK, "The fanart thumbnail should be updated successfully since we are using a valid timestamp, so it should return 204 Ok.");
        }
    }

    async Task<MediaReadModel> AddTestImageFile(MediaBrowserWebApplicationFactory factory, HttpClient client)
    {
        var importClient = new ImportClient(client);
        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();

        const string fileName = "import.webp";
        var filePath = Path.Combine(mediaConfig.ImportDirectory!, fileName);
        await TestFiles.Files.Image(filePath);

        using var response = await importClient.Import(fileName, new()
        {
            Title = "image title",
            OriginalTitle = "original title",
            Description = "description",
            Cast = [],
            Directors = [],
            Genres = [],
            Producers = [],
            Writers = []
        });
        response.EnsureSuccessStatusCode();
        response.Content.ShouldNotBeNull();

        return response.Content;
    }

    const string _noThumbnailTag = "no thumbnail";
    async Task<MediaReadModel> AddTestVideoFile(MediaBrowserWebApplicationFactory factory, HttpClient client)
    {
        var importClient = new ImportClient(client);
        var mediaConfig = factory.Services.GetRequiredService<MediaConfig>();

        const string fileName = "import.mp4";
        var filePath = Path.Combine(mediaConfig.ImportDirectory!, fileName);
        await TestFiles.Files.Mp4(filePath);

        using var response = await importClient.Import(fileName, new()
        {
            Title = "video title",
            OriginalTitle = "original title",
            Description = "description",
            Cast = ["cast", $"cast {_noThumbnailTag}"],
            Directors = ["director", $"director {_noThumbnailTag}"],
            Genres = ["genre", $"genre {_noThumbnailTag}"],
            Producers = ["producer", $"producer {_noThumbnailTag}"],
            Writers = ["writer", $"writer {_noThumbnailTag}"],
            Thumbnail = 0.5
        });
        response.EnsureSuccessStatusCode();
        response.Content.ShouldNotBeNull();

        // The first item in each list should have a thumbnail, and the second item should not.
        // So we check that the first item has a thumbnail file, and we don't check for the second item.
        await TestFiles.Files.Cast(Path.Combine(mediaConfig.CastDirectory, $"{response.Content.Cast[0]}.jpg"));
        await TestFiles.Files.Cast(Path.Combine(mediaConfig.DirectorsDirectory, $"{response.Content.Directors[0]}.jpg"));
        await TestFiles.Files.Cast(Path.Combine(mediaConfig.GenresDirectory, $"{response.Content.Genres[0]}.jpg"));
        await TestFiles.Files.Cast(Path.Combine(mediaConfig.ProducersDirectory, $"{response.Content.Producers[0]}.jpg"));
        await TestFiles.Files.Cast(Path.Combine(mediaConfig.WritersDirectory, $"{response.Content.Writers[0]}.jpg"));

        return response.Content;
    }
}