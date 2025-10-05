namespace MediaBrowser.Media.Cast;

public class CastEntity
{
    [Column("id"), Key, MaxLength(36), Required]
    public required int Id { get; set; } 
    [Column("media_id"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("cast_member"), Required, MaxLength(50)]
    public required string Name { get; init; }
    public required MediaEntity Media { get; init; }
}

public class CastEntityConfiguration : IEntityTypeConfiguration<CastEntity>
{
    public void Configure(EntityTypeBuilder<CastEntity> builder)
    {
        builder.ToTable("media_cast");
        
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Cast)
            .HasForeignKey(e => e.MediaId);
    }
}