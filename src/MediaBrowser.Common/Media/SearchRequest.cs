namespace MediaBrowser.Media;

public class SearchRequest
{
    public bool Descending { get; init; }
    public string? Cast { get; init; }
    public string? Directors { get; init; }
    public string? Genres { get; init; }
    public string? Keywords { get; init; }
    public string? Producers { get; init; }
    public string? Writers { get; init; }
    public Sort Sort { get; init; } = Sort.CreatedOn;
    [Range(0, int.MaxValue)]
    public int Skip { get; init; }
    [Range(1, int.MaxValue)]
    public required int? Take { get; init; }

    public IQueryable<MediaEntity> Apply(IQueryable<MediaEntity> query)
    {
        var castQuery = Cast?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
        if (castQuery is {Length: > 0})
        {
            query = query
                .Where(m => castQuery.Any(n => m.Cast.Any(c => c.Name == n)));
        }

        var directorsQuery = Directors?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
        if (directorsQuery is {Length: > 0})
        {
            query = query
                .Where(m => directorsQuery.Any(n => m.Directors.Any(c => c.Name == n)));
        }

        var genresQuery = Genres?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
        if (genresQuery is {Length: > 0})
        {
            query = query
                .Where(m => genresQuery.Any(n => m.Genres.Any(g => g.Name == n)));
        }

        if (!string.IsNullOrEmpty(Keywords))
        {
            query = query.Where(it =>
                EF.Functions.Like(it.Title, $"%{Keywords}%")
                || EF.Functions.Like(it.OriginalTitle, $"%{Keywords}%")
                || EF.Functions.Like(it.Description, $"%{Keywords}%")
                || EF.Functions.Like(it.Published, $"%{Keywords}%")
                || EF.Functions.Like(it.Md5, $"%{Keywords}%")
                || EF.Functions.Like(it.Path, $"%{Keywords}%")
                || it.Cast.Any(c => EF.Functions.Like(c.Name, $"%{Keywords}%"))
                || it.Directors.Any(d => EF.Functions.Like(d.Name, $"%{Keywords}%"))
                || it.Genres.Any(g => EF.Functions.Like(g.Name, $"%{Keywords}%"))
                || it.Producers.Any(p => EF.Functions.Like(p.Name, $"%{Keywords}%"))
                || it.Writers.Any(w => EF.Functions.Like(w.Name, $"%{Keywords}%")));
        }

        var producersQuery = Producers?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
        if (producersQuery is {Length: > 0})
        {
            query = query
                .Where(m => producersQuery.Any(n => m.Producers.Any(c => c.Name == n)));
        }

        var writersQuery = Writers?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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

public enum Sort
{
    Title,
    CreatedOn,
    Duration,
    UserStarRating
}

[ExcludeFromCodeCoverage(Justification = "POCO")]
public class SearchResponse
{
    public required IReadOnlyList<MediaReadModel> Results { get; init; }

    public required int Count { get; init; }
}