namespace MediaBrowser.Media;

[ApiController, Route("api/[controller]")]
public class MediaController(MediaConfig mediaConfig, MediaDbContext context) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaReadModel>> Get(Guid id)
    {
        var media = await context.MediaJoined.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        return media.ToReadModel(mediaConfig);
    }

    [HttpGet("search")]
    public async Task<SearchResponse> Search([FromQuery] SearchRequest request)
    {
        var query = context.MediaJoined;

        var castQuery = request.Cast?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
        if (castQuery is {Length: > 0})
        {
            query = query
                .Where(m => castQuery.Any(n => m.Cast.Any(c => c.Name == n)));
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
                    it.Title.Contains(request.Keywords)
                    || it.OriginalTitle.Contains(request.Keywords)
                    || it.Description.Contains(request.Keywords)
                    || it.Published.Contains(request.Keywords)
                    || it.Md5.Contains(request.Keywords)
                    || it.Path.Contains(request.Keywords)
                    || it.Cast.Any(c => c.Name.Contains(request.Keywords))
                    || it.Directors.Any(d => d.Name.Contains(request.Keywords))
                    || it.Genres.Any(g => g.Name.Contains(request.Keywords))
                    || it.Producers.Any(p => p.Name.Contains(request.Keywords))
                    || it.Writers.Any(w => w.Name.Contains(request.Keywords)));
        }
        
        var count = await query.CountAsync();
        
        switch (request.Sort)
        {
            case Sort.Title:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Title)
                    : query.OrderBy(m => m.Title);
                break;
            case Sort.CreatedOn:
                query = request.Descending
                    ? query.OrderByDescending(m => m.CreatedOn)
                    : query.OrderBy(m => m.CreatedOn);
                break;
            case Sort.Duration:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Duration)
                    : query.OrderBy(m => m.Duration);
                break;
            case Sort.UserStarRating:
                query = request.Descending
                    ? query.OrderByDescending(m => m.UserStarRating)
                    : query.OrderBy(m => m.UserStarRating);
                break;
        }
        
        query = query.Skip(request.Skip);

        if (request.Take != null)
        {
            query = query.Take(request.Take.Value);
        }
        
        var results = await query.ToListAsync();
        
        return new SearchResponse
        {
            Count = count,
            Results = results.Select(it => it.ToReadModel(mediaConfig)).ToArray()
        };
    }
    
    [HttpGet("cast")]
    public async Task<IReadOnlyList<string>> GetAllCast() =>
        await context.Casts.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("cast/{name}/thumbnail")]
    public Task<ActionResult> GetCastThumbnail(string name) => File(mediaConfig.CastDirectory, $"{name}.jpg");

    [HttpGet("directors")]
    public async Task<IReadOnlyList<string>> GetAllDirectors() =>
        await context.Directors.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("director/{name}/thumbnail")]
    public Task<ActionResult> GetDirectorThumbnail(string name) => File(mediaConfig.DirectorsDirectory, $"{name}.jpg");
    
    [HttpGet("genres")]
    public async Task<IReadOnlyList<string>> GetAllGenres() =>
        await context.Genres.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("genre/{name}/thumbnail")]
    public Task<ActionResult> GetGenreThumbnail(string name) => File(mediaConfig.GenresDirectory, $"{name}.jpg");
    
    [HttpGet("producers")]
    public async Task<IReadOnlyList<string>> GetAllProducers() =>
        await context.Producers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("producer/{name}/thumbnail")]
    public Task<ActionResult> GetProducerThumbnail(string name) => File(mediaConfig.ProducersDirectory, $"{name}.jpg");
    
    [HttpGet("writers")]
    public async Task<IReadOnlyList<string>> GetAllWriters() =>
        await context.Writers.Select(c => c.Name).Distinct().OrderBy(n => n).ToListAsync();

    [HttpGet("writer/{name}/thumbnail")]
    public Task<ActionResult> GetWriterThumbnail(string name) => File(mediaConfig.WritersDirectory, $"{name}.jpg");
    
    [HttpGet("{id:guid}/file")]
    public Task<ActionResult> Stream(Guid id) =>
        File(id, media => $".{media.ToReadModel(mediaConfig).Ffprobe.Ext}", media => media.Mime, true);
    
    [HttpGet("{id:guid}/file/thumbnail-fanart")]
    public Task<ActionResult> StreamFanartThumbnail(Guid id) =>
        File(id, _ => "-fanart.jpg", _ => "image/jpeg");
    
    [HttpGet("{id:guid}/file/thumbnail")]
    public Task<ActionResult> StreamThumbnail(Guid id) =>
        File(id, _ => ".jpg", _ => "image/jpeg");
    
    async Task<ActionResult> File(Guid id,
        Func<MediaEntity, string> extension,
        Func<MediaEntity, string> mime,
        bool etag = false)
    {
        var media = await context.Media.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        
        var filePath = Path.Combine(mediaConfig.MediaDirectory, $"{media.Md5}{extension(media)}");
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        if (etag)
        {
            var etagValue = $"\"{media.Md5}\"";
            if (Request.Headers.ContainsKey("If-None-Match") &&
                Request.Headers["If-None-Match"] == etagValue)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            Response.Headers["ETag"] = etagValue;
        }
        
        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        if (Request.Headers.ContainsKey("If-Modified-Since") &&
            DateTime.TryParse(Request.Headers["If-Modified-Since"], out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers["Last-Modified"] = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), mime(media));
    }

    async Task<ActionResult> File(string directory, string name)
    {
        var filePath = Path.Combine(directory, name);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        if (Request.Headers.ContainsKey("If-Modified-Since") &&
            DateTime.TryParse(Request.Headers["If-Modified-Since"], out var ifModifiedSince) &&
            Math.Abs((lastModified - ifModifiedSince.ToUniversalTime()).TotalSeconds) < 2)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers["Last-Modified"] = lastModified.ToString("R");

        return File(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), "image/jpeg");
    }
}