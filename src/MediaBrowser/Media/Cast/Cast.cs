namespace MediaBrowser.Media.Cast;

public class CastEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("cast_member"), Required, MaxLength(50)]
    public required string CastMember { get; init; }
}

public class CastEntityConfiguration : IEntityTypeConfiguration<CastEntity>
{
    public void Configure(EntityTypeBuilder<CastEntity> builder)
    {
        builder.ToTable("media_cast");
        
        builder.HasIndex(e => new { e.MediaId, e.CastMember })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}