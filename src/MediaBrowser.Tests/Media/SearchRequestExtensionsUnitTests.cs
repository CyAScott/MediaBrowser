namespace MediaBrowser.Media;

public class SearchRequestExtensionsUnitTests
{
    /* NOTE: sorting and keyword filtering logic is tested in the SearchRequestExtensionsIntegrationTests,
     * EF functions and are not unit testable.
     */
    IEnumerable<MediaEntity> GetSampleMediaEntities()
    {
        var mediaId1 = Guid.NewGuid();
        yield return new()
        {
            Id = Guid.NewGuid(),
            Path = "/a",
            Title = "A",
            OriginalTitle = "OrigA",
            Description = "DescA",
            Mime = "video/mp4",
            Size = 100,
            Width = 1920,
            Height = 1080,
            Duration = 10,
            Md5 = "md5A",
            Rating = 3.5,
            UserStarRating = 3,
            Published = "2020",
            CtimeMs = 1,
            MtimeMs = 2,
            CreatedOn = new DateTime(2020, 1, 1),
            UpdatedOn = new DateTime(2020, 1, 2),
            Ffprobe = new()
            {
                Streams = [],
                Format = null
            },
            Cast = new List<CastEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId1,
                    Name = "John",
                    Media = null!
                }
            },
            Directors = new List<DirectorEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId1,
                    Name = "Jane",
                    Media = null!
                }
            },
            Genres = new List<GenreEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId1,
                    Name = "Action",
                    Media = null!
                }
            },
            Producers = new List<ProducerEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId1,
                    Name = "Producer1",
                    Media = null!
                }
            },
            Writers = new List<WriterEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId1,
                    Name = "Writer1",
                    Media = null!
                }
            }
        };
        var mediaId2 = Guid.NewGuid();
        yield return new()
        {
            Id = mediaId2,
            Path = "/b",
            Title = "B",
            OriginalTitle = "OrigB",
            Description = "DescB",
            Mime = "video/mp4",
            Size = 200,
            Width = 1920,
            Height = 1080,
            Duration = 20,
            Md5 = "md5B",
            Rating = 4.5,
            UserStarRating = 5,
            Published = "2021",
            CtimeMs = 3,
            MtimeMs = 4,
            CreatedOn = new DateTime(2021, 1, 1),
            UpdatedOn = new DateTime(2021, 1, 2),
            Ffprobe = new()
            {
                Streams = [],
                Format = null
            },
            Cast = new List<CastEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId2,
                    Name = "Alice",
                    Media = null!
                }
            },
            Directors = new List<DirectorEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId2,
                    Name = "Bob",
                    Media = null!
                }
            },
            Genres = new List<GenreEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId2,
                    Name = "Drama",
                    Media = null!
                }
            },
            Producers = new List<ProducerEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId2,
                    Name = "Producer2",
                    Media = null!
                }
            },
            Writers = new List<WriterEntity>
            {
                new()
                {
                    Id = 0,
                    MediaId = mediaId2,
                    Name = "Writer2",
                    Media = null!
                }
            }
        };
    }

    [Test(Description = "The search features should work with every SQL DB type."),
     TestCase(DbType.MySql),
     TestCase(DbType.Postgres),
     TestCase(DbType.Sqlite),
     TestCase(DbType.SqlServer)]
    public async Task Test(DbType dbType)
    {
        await using var factory = new MediaBrowserWebApplicationFactory(dbType);

        await factory.StartServerAsync();

        await InsertTestData(factory);

        using var scope = factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

        await ApplySortAndPaginationSkipAndTake(db);
        await ApplySortAndPaginationSkipAndTake(db, Sort.Title, false);
        await ApplySortAndPaginationSkipAndTake(db, Sort.Title, true);
        await ApplySortAndPaginationSkipAndTake(db, Sort.CreatedOn, false);
        await ApplySortAndPaginationSkipAndTake(db, Sort.CreatedOn, true);
        await ApplySortAndPaginationSkipAndTake(db, Sort.Duration, false);
        await ApplySortAndPaginationSkipAndTake(db, Sort.Duration, true);
        await ApplySortAndPaginationSkipAndTake(db, Sort.UserStarRating, false);
        await ApplySortAndPaginationSkipAndTake(db, Sort.UserStarRating, true);
        for (var i = 0; i < 10; i++)
        {
            await ApplySortAndPaginationSkipAndTakeRandom(db, seed: 123 * i + 1);
        }
        await CreateQueryFiltersByCast(db.MediaJoined);
        await CreateQueryFiltersByDirectors(db.MediaJoined);
        await CreateQueryFiltersByGenres(db.MediaJoined);
        await CreateQueryFiltersByProducers(db.MediaJoined);
        await CreateQueryFiltersByWriters(db.MediaJoined);
        await CreateQueryNoFiltersReturnsAll(db.MediaJoined);
    }

    async Task InsertTestData(MediaBrowserWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        await db.Media.AddRangeAsync(GetSampleMediaEntities());
        await db.SaveChangesAsync();
    }

    async Task ApplySortAndPaginationSkipAndTake(MediaDbContext db)
    {
        var request = new SearchRequest { Sort = Sort.Title, Descending = false, Skip = 1, Take = 1 };
        var result = await (await request.ApplySortAndPagination(db, db.MediaJoined)).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    async Task ApplySortAndPaginationSkipAndTake(MediaDbContext db, Sort sort, bool descending)
    {
        var request = new SearchRequest { Sort = sort, Descending = descending, Skip = 0, Take = null };
        var result = await (await request.ApplySortAndPagination(db, db.MediaJoined)).ToListAsync();
        result.Count.ShouldBe(2);

        switch (sort)
        {
            case Sort.Title:
                if (descending)
                {
                    result[0].Title.ShouldBe("B");
                    result[1].Title.ShouldBe("A");
                }
                else
                {
                    result[0].Title.ShouldBe("A");
                    result[1].Title.ShouldBe("B");
                }
                break;
            case Sort.CreatedOn:
                if (descending)
                {
                    result[0].CreatedOn.ShouldNotBeNull().Year.ShouldBe(2021);
                    result[1].CreatedOn.ShouldNotBeNull().Year.ShouldBe(2020);
                }
                else
                {
                    result[0].CreatedOn.ShouldNotBeNull().Year.ShouldBe(2020);
                    result[1].CreatedOn.ShouldNotBeNull().Year.ShouldBe(2021);
                }
                break;
            case Sort.Duration:
                if (descending)
                {
                    result[0].Duration.ShouldNotBeNull().ShouldBe(20);
                    result[1].Duration.ShouldNotBeNull().ShouldBe(10);
                }
                else
                {
                    result[0].Duration.ShouldNotBeNull().ShouldBe(10);
                    result[1].Duration.ShouldNotBeNull().ShouldBe(20);
                }
                break;
            case Sort.UserStarRating:
                if (descending)
                {
                    result[0].UserStarRating.ShouldNotBeNull().ShouldBe(5);
                    result[1].UserStarRating.ShouldNotBeNull().ShouldBe(3);
                }
                else
                {
                    result[0].UserStarRating.ShouldNotBeNull().ShouldBe(3);
                    result[1].UserStarRating.ShouldNotBeNull().ShouldBe(5);
                }
                break;
        }
    }

    async Task ApplySortAndPaginationSkipAndTakeRandom(MediaDbContext db, int seed)
    {
        // With a fixed seed, the random order should be consistent across multiple calls, even with different providers that implement the random sorting differently.
        var ascendingRequest = new SearchRequest { Sort = Sort.Random, Descending = false, Seed = seed, Skip = 0, Take = null };
        var ascendingResults = await (await ascendingRequest.ApplySortAndPagination(db, db.MediaJoined)).ToListAsync();
        ascendingResults.Count.ShouldBe(2);

        var descendingRequest = new SearchRequest { Sort = Sort.Random, Descending = true, Seed = seed, Skip = 0, Take = null };
        var descendingResults = await (await descendingRequest.ApplySortAndPagination(db, db.MediaJoined)).ToListAsync();
        descendingResults.Count.ShouldBe(2);

        ascendingResults[0].Id.ShouldBe(descendingResults[1].Id);
        ascendingResults[1].Id.ShouldBe(descendingResults[0].Id);
    }

    async Task CreateQueryFiltersByCast(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Cast = "John", Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("A");
    }

    async Task CreateQueryFiltersByDirectors(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Directors = "Bob", Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    async Task CreateQueryFiltersByGenres(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Genres = "Drama", Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    async Task CreateQueryFiltersByProducers(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Producers = "Producer2", Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    async Task CreateQueryFiltersByWriters(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Writers = "Writer1", Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("A");
    }

    async Task CreateQueryNoFiltersReturnsAll(IQueryable<MediaEntity> query)
    {
        var request = new SearchRequest { Take = 2 };
        var result = await request.CreateQuery(query).ToListAsync();
        result.Count.ShouldBe(2);
    }
}