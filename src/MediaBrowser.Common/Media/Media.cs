namespace MediaBrowser.Media;

[Table("media")]
public class MediaEntity
{
    [Column("id"), Key]
    public required Guid Id { get; init; }

    [Column("path")]
    public required string Path { get; init; }

    [Column("title")]
    public required string Title { get; init; }

    [Column("original_title")]
    public required string OriginalTitle { get; init; }

    [Column("description")]
    public required string Description { get; init; }

    [Column("mime")]
    public required string Mime { get; init; }

    [Column("size")]
    public required long? Size { get; init; }

    [Column("width")]
    public required int? Width { get; init; }

    [Column("height")]
    public required int? Height { get; init; }

    [Column("duration")]
    public required double? Duration { get; init; }

    [Column("md5"), MaxLength(32)]
    public required string Md5 { get; init; }

    [Column("rating")]
    public required double? Rating { get; init; }

    [Column("user_star_rating")]
    public required int? UserStarRating { get; init; }

    [Column("published")]
    public required string Published { get; init; }

    [Column("ctime_ms")]
    public required long CtimeMs { get; init; }

    [Column("mtime_ms")]
    public required long MtimeMs { get; init; }

    [Column("created_on")]
    public required DateTime? CreatedOn { get; init; }

    [Column("updated_on")]
    public required DateTime? UpdatedOn { get; init; }

    [Column("ffprobe")]
    public required FfprobeResponse Ffprobe { get; init; }
    
    public ICollection<CastEntity> Cast { get; set; } = [];
    
    public ICollection<DirectorEntity> Directors { get; set; } = [];
    
    public ICollection<GenreEntity> Genres { get; set; } = [];
    
    public ICollection<ProducerEntity> Producers { get; set; } = [];
    
    public ICollection<WriterEntity> Writers { get; set; } = [];
    
    public MediaReadModel ToReadModel(MediaConfig config) => new()
    {
        Id = Id,
        Path = Path,
        Title = Title,
        OriginalTitle = OriginalTitle,
        Description = Description,
        Mime = Mime,
        Size = Size,
        Width = Width,
        Height = Height,
        Duration = Duration,
        Md5 = Md5,
        Rating = Rating,
        UserStarRating = UserStarRating,
        Published = Published,
        CtimeMs = CtimeMs,
        MtimeMs = MtimeMs,
        CreatedOn = CreatedOn,
        UpdatedOn = UpdatedOn,
        Ffprobe = Ffprobe,
        Cast = Cast.Select(it => it.Name).ToList(),
        Directors = Directors.Select(it => it.Name).ToList(),
        Genres = Genres.Select(it => it.Name).ToList(),
        Producers = Producers.Select(it => it.Name).ToList(),
        Writers = Writers.Select(it => it.Name).ToList(),
        Url = $"/api/Media/{Id}/file",
        ThumbnailUrl = File.Exists(System.IO.Path.Combine(config.MediaDirectory, $"{Md5}.jpg"))
            ? $"/api/Media/{Id}/file/thumbnail" : null,
        FanartThumbnailUrl = File.Exists(System.IO.Path.Combine(config.MediaDirectory, $"{Md5}-fanart.jpg"))
            ? $"/api/Media/{Id}/file/thumbnail-fanart" : null
    };

    public static MediaEntity Create(
        FileInfo fileInfo,
        FfprobeResponse ffprobe,
        ImportMediaRequest request,
        string hash, string mime,
        IEnumerable<string> cast,
        IEnumerable<string> directors,
        IEnumerable<string> genres,
        IEnumerable<string> producers,
        IEnumerable<string> writers,
        Guid? mediaId = null,
        int? height = null, int? width = null,
        long? ctimeMs = null, long? mtimeMs = null)
    {
        var castEntities = new List<CastEntity>();
        var directorEntities = new List<DirectorEntity>();
        var genreEntities = new List<GenreEntity>();
        var producerEntities = new List<ProducerEntity>();
        var writerEntities = new List<WriterEntity>();

        var media = new MediaEntity
        {
            Id = mediaId ?? Guid.CreateVersion7(),
            CtimeMs = ctimeMs ?? Convert.ToInt64((fileInfo.CreationTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            CreatedOn = ctimeMs == null ? fileInfo.CreationTimeUtc : DateTime.UnixEpoch.AddMilliseconds(ctimeMs.Value),
            UpdatedOn = mtimeMs == null ? fileInfo.LastWriteTimeUtc : DateTime.UnixEpoch.AddMilliseconds(mtimeMs.Value),
            Path = fileInfo.Name,
            Title = request.Title,
            OriginalTitle = request.OriginalTitle,
            Description = request.Description,
            Mime = mime,
            Size = fileInfo.Length,
            Width = width ?? ffprobe.Streams?.Select(it => it.Width).OfType<int>().First(),
            Height = height ?? ffprobe.Streams?.Select(it => it.Height).OfType<int>().First(),
            Duration = double.Parse(ffprobe.Format?.Duration ?? "0"),
            Md5 = hash,
            Rating = request.Rating,
            UserStarRating = request.UserStarRating,
            Published = request.Published,
            MtimeMs = mtimeMs ?? Convert.ToInt64((fileInfo.LastWriteTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            Ffprobe = ffprobe,
            
            Cast = castEntities,
            Directors = directorEntities,
            Genres = genreEntities,
            Producers = producerEntities,
            Writers = writerEntities
        };
        
        foreach (var name in cast)
        {
            castEntities.Add(new CastEntity
            {
                Id = 0,
                Media = media,
                MediaId = media.Id,
                Name = name
            });
        }
        
        foreach (var name in directors)
        {
            directorEntities.Add(new DirectorEntity
            {
                Id = 0,
                Media = media,
                MediaId = media.Id,
                Name = name
            });
        }
        
        foreach (var name in genres)
        {
            genreEntities.Add(new GenreEntity
            {
                Id = 0,
                Media = media,
                MediaId = media.Id,
                Name = name
            });
        }
        
        foreach (var name in producers)
        {
            producerEntities.Add(new ProducerEntity
            {
                Id = 0,
                Media = media,
                MediaId = media.Id,
                Name = name
            });
        }
        
        foreach (var name in writers)
        {
            writerEntities.Add(new WriterEntity
            {
                Id = 0,
                Media = media,
                MediaId = media.Id,
                Name = name
            });
        }
        
        return media;
    }
}

public class MediaEntityConfiguration(DbType type) : IEntityTypeConfiguration<MediaEntity>
{
    public void Configure(EntityTypeBuilder<MediaEntity> builder)
    {
        // Configure DateTime with UTC handling for all databases
        builder
            .Property(m => m.CreatedOn)
            .HasConversion(
                value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null,
                value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);

        builder
            .Property(m => m.UpdatedOn)
            .HasConversion(
                value => value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null,
                value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);

        // Configure string length limits for database compatibility
        builder.Property(m => m.Path).HasMaxLength(500);
        builder.Property(m => m.Title).HasMaxLength(200);
        builder.Property(m => m.OriginalTitle).HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Mime).HasMaxLength(100);
        builder.Property(m => m.Published).HasMaxLength(100);
        
        // Ensure MD5 is exactly 32 characters
        builder.Property(m => m.Md5).HasMaxLength(32);

        var ffprobe = builder.Property(m => m.Ffprobe)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<FfprobeResponse>(v, (JsonSerializerOptions?)null)!);
        switch (type)
        {
            case DbType.MySql:
                ffprobe.HasColumnType("JSON");
                break;
            case DbType.Postgres:
                ffprobe.HasColumnType("JSONB");
                break;
            default:
            case DbType.Sqlite:
                ffprobe.HasColumnType("TEXT");
                break;
            case DbType.SqlServer:
                ffprobe.HasColumnType("JSON");
                break;
        }

        // Configure relationships
        builder.HasMany(m => m.Cast)
            .WithOne(c => c.Media)
            .HasForeignKey(c => c.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Directors)
            .WithOne(d => d.Media)
            .HasForeignKey(d => d.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Genres)
            .WithOne(g => g.Media)
            .HasForeignKey(g => g.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Producers)
            .WithOne(p => p.Media)
            .HasForeignKey(p => p.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Writers)
            .WithOne(w => w.Media)
            .HasForeignKey(w => w.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
