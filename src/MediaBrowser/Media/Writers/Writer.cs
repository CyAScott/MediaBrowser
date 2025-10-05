namespace MediaBrowser.Media.Writers;

public class WriterEntity
{
    [Column("id"), Key, MaxLength(36), Required]
    public required int Id { get; set; } 
    [Column("media_id"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("writer"), Required, MaxLength(50)]
    public required string Name { get; init; }
    public required MediaEntity Media { get; init; }
}

public class WriterEntityConfiguration : IEntityTypeConfiguration<WriterEntity>
{
    public void Configure(EntityTypeBuilder<WriterEntity> builder)
    {
        builder.ToTable("media_writers");
        
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Writers)
            .HasForeignKey(e => e.MediaId);
    }
}