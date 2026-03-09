namespace MediaBrowser.Media.Import;

public class ImportInstallerTests
{
    [Test(Description = "If the app is configured to sync on boot, it should read all .nfo files in the media directory and add them to the database.")]
    public async Task ImportTest()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();

        factory.ConfigurationFiles.Add(new()
        {
            {
                "media", new JsonObject
                {
                    {"stopAfterSync", true},
                    {"syncOnBoot", true}
                }
            }
        });

        var config = factory.GetConfiguration();
        var files = await AddTestFiles(config).ToListAsync();

        await factory.StartServerAsync();

        using var scope = factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var count = await db.Media.CountAsync();
        count.ShouldBe(files.Count, "Some files were not imported into the database.");
        foreach (var expected in files)
        {
            var actual = await db.Media.FirstOrDefaultAsync(a => a.Id == expected.Id);
            actual.ShouldNotBeNull("The media entity should have been imported into the database.");
            actual.ShouldNotBeSameAs(expected, "The media entity in the database should not be the same instance as the one we created.");
        }
    }

    async IAsyncEnumerable<MediaEntity> AddTestFiles(IConfiguration config)
    {
        var mediaConfig = new MediaConfig(config);
        var nfo = new Nfo(mediaConfig);

        var hiddenFileMedia = MediaEntityFactory();
        var hiddenFilePath = Path.Combine(mediaConfig.MediaDirectory, $".{hiddenFileMedia.Id}.nfo");
        await nfo.Save(hiddenFileMedia, hiddenFilePath);
        //NOTE: The importer should ignore hidden files, so we won't yield this one.

        var invalidFilePath = Path.Combine(mediaConfig.MediaDirectory, $"{Guid.NewGuid()}.nfo");
        await File.WriteAllTextAsync(invalidFilePath, "This is not valid XML content.", Encoding.UTF8);
        //NOTE: The importer should ignore invalid files, so we won't yield this one.

        foreach (var _ in Enumerable.Range(1, 100))
        {
            var media = MediaEntityFactory();
            var nfoFilePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Id}.nfo");
            await nfo.Save(media, nfoFilePath);

            var videoFilePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.mp4");
            await File.Create(videoFilePath).DisposeAsync();

            yield return media;
        }
    }

    int _autoId;
    MediaEntity MediaEntityFactory()
    {
        var mediaId = Guid.NewGuid();
        var random = TestContext.CurrentContext.Random;
        var nowEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var now = DateTimeOffset.UnixEpoch.AddMilliseconds(nowEpoch).DateTime;
        return new()
        {
            Id = mediaId,
            Path = $"/media/{Guid.NewGuid()}.mp4",
            Title = $"Title {Guid.NewGuid()}",
            OriginalTitle = $"Original {Guid.NewGuid()}",
            Description = $"Description {Guid.NewGuid()}",
            Mime = "video/mp4",
            Size = random.NextBool() ? null : random.NextInt64(1_000_000, 10_000_000),
            Width = random.NextBool() ? null : random.Next(640, 1920),
            Height = random.NextBool() ? null : random.Next(360, 1080),
            Duration = random.NextBool() ? null : random.NextDouble() * 3600,
            Md5 = Convert.ToHexStringLower(Guid.NewGuid().ToByteArray()),
            Rating = random.NextBool() ? null : Math.Round(random.NextDouble() * 10, 1),
            UserStarRating = random.NextBool() ? null : random.Next(1, 5),
            Published = now.AddDays(-random.Next(0, 365)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            CtimeMs = nowEpoch - random.NextInt64(0, 365 * 24 * 60 * 1000),
            MtimeMs = nowEpoch,
            CreatedOn = now.AddDays(-random.Next(0, 365)),
            UpdatedOn = now,
            Ffprobe = RandomFfprobeResponse(),
            Directors = [new()
                { Id = _autoId++, MediaId = mediaId, Name = $"Director {Guid.NewGuid()}" }],
            Genres = [new()
                { Id = _autoId++, MediaId = mediaId, Name = $"Genre {Guid.NewGuid()}" }],
            Producers = [new()
                { Id = _autoId++, MediaId = mediaId, Name = $"Producer {Guid.NewGuid()}" }],
            Writers = [new()
                { Id = _autoId++, MediaId = mediaId, Name = $"Writer {Guid.NewGuid()}" }]
        };
    }

    FfprobeResponse RandomFfprobeResponse()
    {
        var random = TestContext.CurrentContext.Random;
        return new()
        {
            Streams = new List<Stream>
            {
                new()
                {
                    Index = 0,
                    CodecName = "h264",
                    CodecLongName = "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
                    Profile = "High",
                    CodecType = "video",
                    Width = random.Next(640, 1920),
                    Height = random.Next(360, 1080),
                    Duration = (random.NextDouble() * 3600).ToString("F2", CultureInfo.InvariantCulture),
                    BitRate = random.Next(1_000_000, 10_000_000).ToString(CultureInfo.InvariantCulture),
                    SampleRate = random.Next(22050, 48000).ToString(CultureInfo.InvariantCulture),
                    Channels = random.Next(1, 8),
                    Tags = new()
                    {
                        CreationTime = DateTime.UtcNow.AddDays(-random.Next(0, 365)).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                        Language = "eng",
                        HandlerName = "VideoHandler",
                        VendorId = Guid.NewGuid().ToString()
                    }
                }
            },
            Format = new()
            {
                Filename = $"/media/{Guid.NewGuid()}.mp4",
                NbStreams = 1,
                FormatName = "mov,mp4,m4a,3gp,3g2,mj2",
                FormatLongName = "QuickTime / MOV",
                StartTime = "0.000000",
                Duration = (random.NextDouble() * 3600).ToString("F2", CultureInfo.InvariantCulture),
                Size = random.NextInt64(1_000_000, 10_000_000).ToString(CultureInfo.InvariantCulture),
                BitRate = random.Next(1_000_000, 10_000_000).ToString(CultureInfo.InvariantCulture),
                ProbeScore = random.Next(50, 100),
                Tags = new()
                {
                    MajorBrand = "mp42",
                    MinorVersion = "0",
                    CompatibleBrands = "isom,mp42",
                    CreationTime = DateTime.UtcNow.AddDays(-random.Next(0, 365)).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
                }
            }
        };
    }
}