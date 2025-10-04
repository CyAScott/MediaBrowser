namespace MediaBrowser.Media.Genres;

public class GenreEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("genre"), Required, MaxLength(50)]
    public required string Genre { get; init; }
}

public class GenreEntityConfiguration : IEntityTypeConfiguration<GenreEntity>
{
    public void Configure(EntityTypeBuilder<GenreEntity> builder)
    {
        builder.ToTable("media_genres");
        
        builder.HasIndex(e => new { e.MediaId, e.Genre })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}