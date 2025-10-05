namespace MediaBrowser.Media;

[ApiController, Route("api/[controller]")]
public class MediaController(MediaConfig mediaConfig, MediaDbContext context) : ControllerBase
{
    [HttpGet("/{id:guid}")]
    public async Task<ActionResult<MediaReadModel>> Get(Guid id)
    {
        var media = await context.MediaJoin.Where(m => m.Media.Id == id).FirstOrDefaultAsync();
        if (media == null)
        {
            return NotFound();
        }
        return media.ToReadModel();
    }

    [HttpGet("/search")]
    public async Task<SearchResponse> Search([FromQuery] SearchRequest request)
    {
        var castQuery = request.Cast?.Split(',');
        var genresQuery = request.Genres?.Split(',');
        var query = context.MediaJoin;

        if (!string.IsNullOrEmpty(request.Keywords))
        {
            query = query.Where(it =>
                    it.Media.Title.Contains(request.Keywords)
                    || it.Media.OriginalTitle.Contains(request.Keywords)
                    || it.Media.Description.Contains(request.Keywords)
                    || it.Media.Published.Contains(request.Keywords)
                    || it.Media.CtimeMs.Contains(request.Keywords)
                    || it.Media.Md5.Contains(request.Keywords)
                    || it.Media.Path.Contains(request.Keywords)
                    || it.Cast.Any(c => c.Contains(request.Keywords))
                    || castQuery == null || castQuery.Any(c => it.Cast.Contains(c))
                    || it.Directors.Any(d => d.Contains(request.Keywords))
                    || it.Genres.Any(g => g.Contains(request.Keywords))
                    || genresQuery == null || genresQuery.Any(g => it.Genres.Contains(g))
                    || it.Producers.Any(p => p.Contains(request.Keywords))
                    || it.Writers.Any(w => w.Contains(request.Keywords)));
        }

        switch (request.Sort)
        {
            case Sort.Title:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Media.Title)
                    : query.OrderBy(m => m.Media.Title);
                break;
            case Sort.CreatedOn:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Media.CreatedOn)
                    : query.OrderBy(m => m.Media.CreatedOn);
                break;
            case Sort.Duration:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Media.Duration)
                    : query.OrderBy(m => m.Media.Duration);
                break;
            case Sort.UserStarRating:
                query = request.Descending
                    ? query.OrderByDescending(m => m.Media.UserStarRating)
                    : query.OrderBy(m => m.Media.UserStarRating);
                break;
        }
        
        var count = await query.CountAsync();
        
        query = query.Skip(request.Skip).Take(request.Take);
        
        var results = await query.ToListAsync();
        
        return new SearchResponse
        {
            Count = count,
            Results = results.Select(it => it.ToReadModel()).ToArray()
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