namespace MediaBrowser.Media.Producers;

public class ProducerEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("producer"), Required, MaxLength(50)]
    public required string Producer { get; init; }
}

public class ProducerEntityConfiguration : IEntityTypeConfiguration<ProducerEntity>
{
    public void Configure(EntityTypeBuilder<ProducerEntity> builder)
    {
        builder.ToTable("media_producers");
        
        builder.HasIndex(e => new { e.MediaId, e.Producer })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}