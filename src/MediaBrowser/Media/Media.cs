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

	public string Extension() => "mp4";

	[Column("size")]
	public required long? Size { get; init; }

	[Column("width")]
	public required int? Width { get; init; }

	[Column("height")]
	public required int? Height { get; init; }

	[Column("duration")]
	public required double? Duration { get; init; }

	[Column("md5")]
	public required string Md5 { get; init; }

	[Column("rating")]
	public required double? Rating { get; init; }

	[Column("user_star_rating")]
	public required int? UserStarRating { get; init; }

	[Column("published")]
	public required string Published { get; init; }

	[Column("ctime_ms")]
	public required string CtimeMs { get; init; }

	[Column("mtime_ms")]
	public required string MtimeMs { get; init; }

	[Column("created_on")]
	public required DateTime? CreatedOn { get; init; }

	[Column("updated_on")]
	public required DateTime? UpdatedOn { get; init; }

	[Column("ffprobe")]
	public required string Ffprobe { get; init; }
}

public class MediaJoin
{
	public required MediaEntity Media { get; init; }
	public string[] Cast { get; init; } = [];
	public string[] Directors { get; init; } = [];
	public string[] Genres { get; init; } = [];
	public string[] Producers { get; init; } = [];
	public string[] Writers { get; init; } = [];

	public MediaReadModel ToReadModel() => new()
	{
		Id = Media.Id,
		Path = Media.Path,
		Title = Media.Title,
		OriginalTitle = Media.OriginalTitle,
		Description = Media.Description,
		Mime = Media.Mime,
		Size = Media.Size,
		Width = Media.Width,
		Height = Media.Height,
		Duration = Media.Duration,
		Md5 = Media.Md5,
		Rating = Media.Rating,
		UserStarRating = Media.UserStarRating,
		Published = Media.Published,
		CtimeMs = Media.CtimeMs,
		MtimeMs = Media.MtimeMs,
		CreatedOn = Media.CreatedOn,
		UpdatedOn = Media.UpdatedOn,
		Ffprobe = JsonSerializer.Deserialize<FfprobeResponse>(Media.Ffprobe)!,
		Cast = Cast,
		Directors = Directors,
		Genres = Genres,
		Producers = Producers,
		Writers = Writers
	};
}

public class MediaEntityConfiguration : IEntityTypeConfiguration<MediaEntity>
{
	public void Configure(EntityTypeBuilder<MediaEntity> builder)
	{
		builder.ToTable("media");
		
		builder
			.Property(it => it.CreatedOn)
			.HasConversion(
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified),
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
		
		builder
			.Property(it => it.UpdatedOn)
			.HasConversion(
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified),
				value => value == null ? value : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
	}
}
