using MediaBrowser.Media.Cast;
using MediaBrowser.Media.Directors;
using MediaBrowser.Media.Genres;
using MediaBrowser.Media.Producers;
using MediaBrowser.Media.Writers;

namespace MediaBrowser.Media;

public class SearchRequestExtensionsUnitTests
{
    IQueryable<MediaEntity> GetSampleMediaEntities() => new List<MediaEntity>
    {
        new()
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
            Ffprobe = null!,
            Cast = new List<CastEntity> { new()
                { Id = 1, MediaId = Guid.NewGuid(), Name = "John", Media = null! } },
            Directors = new List<DirectorEntity> { new()
                { Id = 1, MediaId = Guid.NewGuid(), Name = "Jane", Media = null! } },
            Genres = new List<GenreEntity> { new()
                { Id = 1, MediaId = Guid.NewGuid(), Name = "Action", Media = null! } },
            Producers = new List<ProducerEntity> { new()
                { Id = 1, MediaId = Guid.NewGuid(), Name = "Producer1", Media = null! } },
            Writers = new List<WriterEntity> { new()
                { Id = 1, MediaId = Guid.NewGuid(), Name = "Writer1", Media = null! } }
        },
        new()
        {
            Id = Guid.NewGuid(),
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
            Ffprobe = null!,
            Cast = new List<CastEntity> { new()
                { Id = 2, MediaId = Guid.NewGuid(), Name = "Alice", Media = null! } },
            Directors = new List<DirectorEntity> { new()
                { Id = 2, MediaId = Guid.NewGuid(), Name = "Bob", Media = null! } },
            Genres = new List<GenreEntity> { new()
                { Id = 2, MediaId = Guid.NewGuid(), Name = "Drama", Media = null! } },
            Producers = new List<ProducerEntity> { new()
                { Id = 2, MediaId = Guid.NewGuid(), Name = "Producer2", Media = null! } },
            Writers = new List<WriterEntity> { new()
                { Id = 2, MediaId = Guid.NewGuid(), Name = "Writer2", Media = null! } }
        }
    }.AsQueryable();

    [Test]
    public void ApplySortAndPagination_SortsByTitleAscending()
    {
        var request = new SearchRequest { Sort = Sort.Title, Descending = false, Skip = 0, Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.ApplySortAndPagination(query).ToList();
        result[0].Title.ShouldBe("A");
        result[1].Title.ShouldBe("B");
    }

    [Test]
    public void ApplySortAndPagination_SortsByTitleDescending()
    {
        var request = new SearchRequest { Sort = Sort.Title, Descending = true, Skip = 0, Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.ApplySortAndPagination(query).ToList();
        result[0].Title.ShouldBe("B");
        result[1].Title.ShouldBe("A");
    }

    [Test]
    public void ApplySortAndPagination_SkipAndTake()
    {
        var request = new SearchRequest { Sort = Sort.Title, Descending = false, Skip = 1, Take = 1 };
        var query = GetSampleMediaEntities();
        var result = request.ApplySortAndPagination(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    [Test]
    public void CreateQuery_FiltersByCast()
    {
        var request = new SearchRequest { Cast = "John", Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("A");
    }

    [Test]
    public void CreateQuery_FiltersByDirectors()
    {
        var request = new SearchRequest { Directors = "Bob", Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    [Test]
    public void CreateQuery_FiltersByGenres()
    {
        var request = new SearchRequest { Genres = "Drama", Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    [Test]
    public void CreateQuery_FiltersByProducers()
    {
        var request = new SearchRequest { Producers = "Producer2", Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("B");
    }

    [Test]
    public void CreateQuery_FiltersByWriters()
    {
        var request = new SearchRequest { Writers = "Writer1", Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("A");
    }

    [Test]
    public void CreateQuery_NoFilters_ReturnsAll()
    {
        var request = new SearchRequest { Take = 2 };
        var query = GetSampleMediaEntities();
        var result = request.CreateQuery(query).ToList();
        result.Count.ShouldBe(2);
    }
}