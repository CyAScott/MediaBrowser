namespace MediaBrowser.Media.Producers;

public class ProducerEntity
{
    [Column("id"), Key, MaxLength(36), Required]
    public required int Id { get; set; } 
    [Column("media_id"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("producer"), Required, MaxLength(50)]
    public required string Name { get; init; }
    public required MediaEntity Media { get; init; }
}

public class ProducerEntityConfiguration : IEntityTypeConfiguration<ProducerEntity>
{
    public void Configure(EntityTypeBuilder<ProducerEntity> builder)
    {
        builder.ToTable("media_producers");
        
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Producers)
            .HasForeignKey(e => e.MediaId);
    }
}