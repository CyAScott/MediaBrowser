namespace MediaBrowser.Media;

[ApiController, Route("api/[controller]")]
public class MediaController(MediaConfig mediaConfig, MediaDbContext context) : ControllerBase
{
    [HttpGet("/{id:guid}")]
    public async Task<ActionResult<MediaReadModel>> Get(Guid id)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        return await CreateReadModel(media);
    }

    async Task<MediaReadModel> CreateReadModel(MediaEntity media) =>
        media.ToReadModel(
            cast: await context.Casts.Where(c => c.MediaId == media.Id).Select(c => c.CastMember).ToArrayAsync(),
            directors: await context.Directors.Where(d => d.MediaId == media.Id).Select(d => d.Director).ToArrayAsync(),
            genres: await context.Genres.Where(g => g.MediaId == media.Id).Select(g => g.Genre).ToArrayAsync(),
            producers: await context.Producers.Where(p => p.MediaId == media.Id).Select(p => p.Producer).ToArrayAsync(),
            writers: await context.Writers.Where(w => w.MediaId == media.Id).Select(w => w.Writer).ToArrayAsync());
    
    [HttpGet("/search")]
    public async Task<SearchResponse> Search([FromQuery] SearchRequest request)
    {
        var castQuery = request.Cast?.Split(',');
        var genresQuery = request.Genres?.Split(',');

        var query = string.IsNullOrEmpty(request.Keywords)
            ? context.Media
            : context.Media.Where(m => 
                context.Casts.Where(it => it.CastMember.Contains(request.Keywords) && (castQuery == null || castQuery.Contains(it.CastMember))).Select(it => it.MediaId).Distinct()
                    .Concat(
                context.Directors.Where(it => it.Director.Contains(request.Keywords)).Select(it => it.MediaId).Distinct()
                    .Concat(
                context.Genres.Where(it => it.Genre.Contains(request.Keywords) && (genresQuery == null || genresQuery.Contains(it.Genre))).Select(it => it.MediaId).Distinct()
                    .Concat(
                context.Producers.Where(it => it.Producer.Contains(request.Keywords)).Select(it => it.MediaId).Distinct()
                    .Concat(
                context.Writers.Where(it => it.Writer.Contains(request.Keywords)).Select(it => it.MediaId).Distinct()))))
                    .Contains(m.Id));

        if (!string.IsNullOrEmpty(request.Keywords))
        {
            query = query.Where(m => m.Title.Contains(request.Keywords)
                || m.OriginalTitle.Contains(request.Keywords)
                || m.Description.Contains(request.Keywords)
                || m.Published.Contains(request.Keywords)
                || m.CtimeMs.Contains(request.Keywords)
                || m.MtimeMs.Contains(request.Keywords)
                || m.Md5.Contains(request.Keywords)
                || m.Mime.Contains(request.Keywords)
                || m.Path.Contains(request.Keywords));
        }

        switch (request.Sort)
        {
            case Sort.Title:
                query = request.Descending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title);
                break;
            case Sort.CreatedOn:
                query = request.Descending ? query.OrderByDescending(m => m.CreatedOn) : query.OrderBy(m => m.CreatedOn);
                break;
            case Sort.Duration:
                query = request.Descending ? query.OrderByDescending(m => m.Duration) : query.OrderBy(m => m.Duration);
                break;
            case Sort.UserStarRating:
                query = request.Descending ? query.OrderByDescending(m => m.UserStarRating) : query.OrderBy(m => m.UserStarRating);
                break;
        }
        
        var count = await query.CountAsync();
        
        query = query.Skip(request.Skip).Take(request.Take);
        
        var results = await query.ToListAsync();

        var mediaIds = results.Select(r => r.Id).ToArray();

        var cast = await context.Casts
            .Where(c => mediaIds.Contains(c.MediaId))
            .GroupBy(it => it.MediaId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(it => it.CastMember).ToArray());
        var directors = await context.Directors
            .Where(d => mediaIds.Contains(d.MediaId))
            .GroupBy(it => it.MediaId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(it => it.Director).ToArray());
        var genres = await context.Genres
            .Where(g => mediaIds.Contains(g.MediaId))
            .GroupBy(it => it.MediaId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(it => it.Genre).ToArray());
        var producers = await context.Producers
            .Where(p => mediaIds.Contains(p.MediaId))
            .GroupBy(it => it.MediaId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(it => it.Producer).ToArray());
        var writers = await context.Writers
            .Where(w => mediaIds.Contains(w.MediaId))
            .GroupBy(it => it.MediaId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(it => it.Writer).ToArray());
        
        return new SearchResponse
        {
            Count = count,
            Results = results
                .Select(it => it.ToReadModel(
                    cast: cast.TryGetValue(it.Id, out var castValue) ? castValue : [],
                    directors: directors.TryGetValue(it.Id, out var directorValue) ? directorValue : [],
                    genres: genres.TryGetValue(it.Id, out var genreValue) ? genreValue : [],
                    producers: producers.TryGetValue(it.Id, out var producerValue) ? producerValue : [],
                    writers: writers.TryGetValue(it.Id, out var writerValue) ? writerValue : [])).ToArray()
        };
    }
    
    [HttpGet("/{id:guid}/file")]
    public async Task<ActionResult> Stream(Guid id)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        
        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.{media.Extension()}");
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), media.Mime);
    }
    
    [HttpGet("/{id:guid}/thumbnail-fanart")]
    public async Task<ActionResult> StreamFanartThumbnail(Guid id)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        
        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}-fanart.jpg");
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), media.Mime);
    }
    
    [HttpGet("/{id:guid}/thumbnail")]
    public async Task<ActionResult> StreamThumbnail(Guid id)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        
        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}.jpg");
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), media.Mime);
    }
}