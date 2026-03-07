namespace MediaBrowser.Media;

[Table("media"), Equatable, ExcludeFromCodeCoverage(Justification = "POCO")]
public partial class MediaEntity
{
    [Column("id"), JsonPropertyName("id"), Key]
    public required Guid Id { get; init; }

    [Column("path"), JsonPropertyName("path")]
    public required string Path { get; init; }

    [Column("title"), JsonPropertyName("title")]
    public required string Title { get; set; }

    [Column("original_title"), JsonPropertyName("originalTitle")]
    public required string OriginalTitle { get; set; }

    [Column("description"), JsonPropertyName("description")]
    public required string Description { get; set; }

    [Column("mime"), JsonPropertyName("mime")]
    public required string Mime { get; init; }

    [Column("size"), JsonPropertyName("size")]
    public required long? Size { get; init; }

    [Column("width"), JsonPropertyName("width")]
    public required int? Width { get; init; }

    [Column("height"), JsonPropertyName("height")]
    public required int? Height { get; init; }

    [Column("duration"), JsonPropertyName("duration")]
    public required double? Duration { get; set; }

    [Column("md5"), JsonPropertyName("md5"), MaxLength(32)]
    public required string Md5 { get; init; }

    [Column("rating"), JsonPropertyName("rating")]
    public required double? Rating { get; set; }

    [Column("user_star_rating"), JsonPropertyName("userStarRating")]
    public required int? UserStarRating { get; set; }

    [Column("published"), JsonPropertyName("published")]
    public required string Published { get; init; }

    [Column("ctime_ms"), JsonPropertyName("ctimeMs")]
    public required long CtimeMs { get; init; }

    [Column("mtime_ms"), JsonPropertyName("mtimeMs")]
    public required long MtimeMs { get; init; }

    [Column("created_on"), JsonPropertyName("createdOn")]
    public required DateTime? CreatedOn { get; init; }

    [Column("updated_on"), JsonPropertyName("updatedOn")]
    public required DateTime? UpdatedOn { get; init; }

    [Column("ffprobe"), JsonPropertyName("ffprobe")]
    public required FfprobeResponse Ffprobe { get; set; }

    [Column("thumbnail"), JsonPropertyName("thumbnail")]
    public double? Thumbnail { get; set; }

    [JsonPropertyName("cast"), UnorderedEquality]
    public ICollection<CastEntity> Cast { get; set; } = [];

    [JsonPropertyName("directors"), UnorderedEquality]
    public ICollection<DirectorEntity> Directors { get; set; } = [];

    [JsonPropertyName("genres"), UnorderedEquality]
    public ICollection<GenreEntity> Genres { get; set; } = [];

    [JsonPropertyName("producers"), UnorderedEquality]
    public ICollection<ProducerEntity> Producers { get; set; } = [];

    [JsonPropertyName("writers"), UnorderedEquality]
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
        Thumbnail = Thumbnail,
        ThumbnailUrl = Mime.StartsWith("image/", StringComparison.InvariantCulture) || File.Exists(System.IO.Path.Combine(config.MediaDirectory, $"{Md5}.jpg"))
            ? $"/api/Media/{Id}/file/thumbnail" : null,
        FanartThumbnailUrl = Mime.StartsWith("image/", StringComparison.InvariantCulture) || File.Exists(System.IO.Path.Combine(config.MediaDirectory, $"{Md5}-fanart.jpg"))
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
        double? thumbnail = null,
        int? height = null, int? width = null,
        long? ctimeMs = null, long? mtimeMs = null)
    {
        var castEntities = new List<CastEntity>();
        var directorEntities = new List<DirectorEntity>();
        var genreEntities = new List<GenreEntity>();
        var producerEntities = new List<ProducerEntity>();
        var writerEntities = new List<WriterEntity>();

        var createdOn = ctimeMs == null ? fileInfo.CreationTimeUtc : DateTime.UnixEpoch.AddMilliseconds(ctimeMs.Value);

        var media = new MediaEntity
        {
            Id = mediaId ?? Guid.CreateVersion7(),
            CtimeMs = ctimeMs ?? Convert.ToInt64((fileInfo.CreationTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            CreatedOn = createdOn,
            UpdatedOn = mtimeMs == null ? fileInfo.LastWriteTimeUtc : DateTime.UnixEpoch.AddMilliseconds(mtimeMs.Value),
            Path = fileInfo.Name,
            Title = request.Title,
            OriginalTitle = request.OriginalTitle,
            Description = request.Description,
            Mime = mime,
            Size = fileInfo.Length,
            Width = width ?? ffprobe.Streams?.Select(it => it.Width).OfType<int>().Cast<int?>().FirstOrDefault(),
            Height = height ?? ffprobe.Streams?.Select(it => it.Height).OfType<int>().Cast<int?>().FirstOrDefault(),
            Duration = double.Parse(ffprobe.Format?.Duration ?? "0", CultureInfo.InvariantCulture),
            Md5 = hash,
            Rating = request.Rating,
            UserStarRating = request.UserStarRating,
            Published = createdOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            MtimeMs = mtimeMs ?? Convert.ToInt64((fileInfo.LastWriteTimeUtc - DateTime.UnixEpoch).TotalMilliseconds),
            Ffprobe = ffprobe,
            Thumbnail = thumbnail,

            Cast = castEntities,
            Directors = directorEntities,
            Genres = genreEntities,
            Producers = producerEntities,
            Writers = writerEntities
        };

        castEntities.AddRange(cast
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new CastEntity {Id = 0, Media = media, MediaId = media.Id, Name = name}));
        directorEntities.AddRange(directors
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new DirectorEntity {Id = 0, Media = media, MediaId = media.Id, Name = name}));
        genreEntities.AddRange(genres
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new GenreEntity {Id = 0, Media = media, MediaId = media.Id, Name = name}));
        producerEntities.AddRange(producers
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new ProducerEntity {Id = 0, Media = media, MediaId = media.Id, Name = name}));
        writerEntities.AddRange(writers
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new WriterEntity {Id = 0, Media = media, MediaId = media.Id, Name = name}));

        return media;
    }

    public void Update(UpdateMediaRequest request)
    {
        Description = request.Description;
        OriginalTitle = request.OriginalTitle;
        Rating = request.Rating;
        Title = request.Title;
        UserStarRating = request.UserStarRating;

        Cast = request.Cast
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new CastEntity {Id = 0, Media = this, MediaId = Id, Name = name}).ToList();
        Directors = request.Directors
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new DirectorEntity {Id = 0, Media = this, MediaId = Id, Name = name}).ToList();
        Genres = request.Genres
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new GenreEntity {Id = 0, Media = this, MediaId = Id, Name = name}).ToList();
        Producers = request.Producers
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new ProducerEntity {Id = 0, Media = this, MediaId = Id, Name = name}).ToList();
        Writers = request.Writers
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct()
            .Select(name => new WriterEntity {Id = 0, Media = this, MediaId = Id, Name = name}).ToList();
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
                ffprobe.HasColumnType("NVARCHAR(MAX)");
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
