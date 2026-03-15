namespace MediaBrowser.Media;

[ExcludeFromCodeCoverage(Justification = "POCO")]
public record SearchRequest
{
    public bool Descending { get; init; }
    public string? Cast { get; init; }
    public string? Directors { get; init; }
    public string? Genres { get; init; }
    public string? Keywords { get; init; }
    public string? Producers { get; init; }
    /// <summary>
    /// This is a seed value used to ensure pagination is consistent across multiple random sort requests.
    /// </summary>
    public int Seed { get; init; }
    public string? Writers { get; init; }
    public Sort Sort { get; init; } = Sort.CreatedOn;
    [Range(0, int.MaxValue)]
    public int Skip { get; init; }
    [Range(1, int.MaxValue)]
    public required int? Take { get; init; }
}

public static class SearchRequestExtensions
{
    extension(SearchRequest request)
    {
        public async Task<IQueryable<MediaEntity>> ApplySortAndPagination(MediaDbContext context, IQueryable<MediaEntity> query)
        {
            var isDescending = request.Descending;

            if (request.Sort == Sort.Random && context.Type == DbType.Postgres)
            {
                // PostgreSQL's random() is not seedable per-query, but setseed() sets the seed for the session.
                // To achieve deterministic pagination, we set the seed at the start of each request.
                await context.Database.ExecuteSqlRawAsync("SELECT setseed({0})", request.Seed / (double)int.MaxValue);
            }

            query = request.Sort switch
            {
                Sort.Title => isDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                Sort.CreatedOn => isDescending ? query.OrderByDescending(m => m.CreatedOn) : query.OrderBy(m => m.CreatedOn),
                Sort.Duration => isDescending ? query.OrderByDescending(m => m.Duration) : query.OrderBy(m => m.Duration),
                Sort.UserStarRating => isDescending ? query.OrderByDescending(m => m.UserStarRating) : query.OrderBy(m => m.UserStarRating),
                // Default random sort
                _ => context.Type switch
                {
                    DbType.MySql => isDescending
                        ? query.OrderByDescending(m => RandomDbFunctions.MySqlRand(request.Seed))
                        : query.OrderBy(m => RandomDbFunctions.MySqlRand(request.Seed)),
                    DbType.Postgres => isDescending
                        ? query.OrderByDescending(m => RandomDbFunctions.PgRandom())
                        : query.OrderBy(m => RandomDbFunctions.PgRandom()),
                    DbType.SqlServer => isDescending
                        ? query.OrderByDescending(m => RandomDbFunctions.SqlChecksum(request.Seed, m.Id))
                        : query.OrderBy(m => RandomDbFunctions.SqlChecksum(request.Seed, m.Id)),
                    // Default SQLite
                    _ => isDescending
                        ? query.OrderByDescending(m => RandomDbFunctions.SqliteSeededRandom(request.Seed, m.Id))
                        : query.OrderBy(m => RandomDbFunctions.SqliteSeededRandom(request.Seed, m.Id))
                }
            };

            query = query.Skip(request.Skip);

            if (request.Take != null)
            {
                query = query.Take(request.Take.Value);
            }

            return query;
        }

        public IQueryable<MediaEntity> CreateQuery(IQueryable<MediaEntity> query)
        {
            var castQuery = request.Cast?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (castQuery is {Length: > 0})
            {
                query = query
                    .Where(m => castQuery.Any(n => m.Cast.Any(c => c.Name == n)));
            }

            var directorsQuery = request.Directors?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (directorsQuery is {Length: > 0})
            {
                query = query
                    .Where(m => directorsQuery.Any(n => m.Directors.Any(c => c.Name == n)));
            }

            var genresQuery = request.Genres?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (genresQuery is {Length: > 0})
            {
                query = query
                    .Where(m => genresQuery.Any(n => m.Genres.Any(g => g.Name == n)));
            }

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(it =>
                    EF.Functions.Like(it.Title, $"%{request.Keywords}%")
                    || EF.Functions.Like(it.OriginalTitle, $"%{request.Keywords}%")
                    || EF.Functions.Like(it.Description, $"%{request.Keywords}%")
                    || EF.Functions.Like(it.Published, $"%{request.Keywords}%")
                    || EF.Functions.Like(it.Md5, $"%{request.Keywords}%")
                    || EF.Functions.Like(it.Path, $"%{request.Keywords}%")
                    || it.Cast.Any(c => EF.Functions.Like(c.Name, $"%{request.Keywords}%"))
                    || it.Directors.Any(d => EF.Functions.Like(d.Name, $"%{request.Keywords}%"))
                    || it.Genres.Any(g => EF.Functions.Like(g.Name, $"%{request.Keywords}%"))
                    || it.Producers.Any(p => EF.Functions.Like(p.Name, $"%{request.Keywords}%"))
                    || it.Writers.Any(w => EF.Functions.Like(w.Name, $"%{request.Keywords}%")));
            }

            var producersQuery = request.Producers?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (producersQuery is {Length: > 0})
            {
                query = query
                    .Where(m => producersQuery.Any(n => m.Producers.Any(c => c.Name == n)));
            }

            var writersQuery = request.Writers?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (writersQuery is {Length: > 0})
            {
                query = query
                    .Where(m => writersQuery.Any(n => m.Writers.Any(c => c.Name == n)));
            }

            return query;
        }
    }
}

public enum Sort
{
    Title,
    CreatedOn,
    Duration,
    UserStarRating,
    Random
}

[Equatable, ExcludeFromCodeCoverage(Justification = "POCO")]
public partial class SearchResponse
{
    [OrderedEquality]
    public required IReadOnlyList<MediaReadModel> Results { get; init; }

    public required int Count { get; init; }
}