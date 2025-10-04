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

	public MediaReadModel ToReadModel(
		string[] cast,
		string[] directors,
		string[] genres,
		string[] producers,
		string[] writers) => new()
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
		Cast = cast,
		Directors = directors,
		Genres = genres,
		Producers = producers,
		Writers = writers
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
