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
