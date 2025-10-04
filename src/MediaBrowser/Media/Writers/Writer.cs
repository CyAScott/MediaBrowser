namespace MediaBrowser.Media.Writers;

public class WriterEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("writer"), Required, MaxLength(50)]
    public required string Writer { get; init; }
}

public class WriterEntityConfiguration : IEntityTypeConfiguration<WriterEntity>
{
    public void Configure(EntityTypeBuilder<WriterEntity> builder)
    {
        builder.ToTable("media_writers");
        
        builder.HasIndex(e => new { e.MediaId, e.Writer })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}