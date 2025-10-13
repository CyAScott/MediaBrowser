namespace MediaBrowser.Media.Writers;

[Table("media_writers")]
public class WriterEntity
{
    [Column("id"), Key, Required,
     DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; set; } 
    [Column("media_id"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("writer"), Required, MaxLength(50)]
    public required string Name { get; init; }
    public MediaEntity Media { get; init; } = null!;
}

public class WriterEntityConfiguration : IEntityTypeConfiguration<WriterEntity>
{
    public void Configure(EntityTypeBuilder<WriterEntity> builder)
    {
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Writers)
            .HasForeignKey(e => e.MediaId);
    }
}