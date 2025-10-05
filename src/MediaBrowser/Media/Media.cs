namespace MediaBrowser.Media;

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
	public required string Ffprobe { get; init; }
	
	public ICollection<CastEntity> Cast { get; init; } = [];
	
	public ICollection<DirectorEntity> Directors { get; init; } = [];
	
	public ICollection<GenreEntity> Genres { get; init; } = [];
	
	public ICollection<ProducerEntity> Producers { get; init; } = [];
	
	public ICollection<WriterEntity> Writers { get; init; } = [];
	
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
		Ffprobe = JsonSerializer.Deserialize<FfprobeResponse>(Ffprobe)!,
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
}

public class MediaEntityConfiguration : IEntityTypeConfiguration<MediaEntity>
{
	public void Configure(EntityTypeBuilder<MediaEntity> builder)
	{
		builder.ToTable("media");
		
		builder
			.Property(m => m.Id)
			.HasConversion<string>(
				value => value.ToString(),
				value => Guid.Parse(value));

		builder
			.Property(m => m.CreatedOn)
			.HasConversion(
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified),
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));

		builder
			.Property(m => m.UpdatedOn)
			.HasConversion(
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified),
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));

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
