namespace MediaBrowser.Media.Cast;

[Table("media_cast"), ExcludeFromCodeCoverage(Justification = "POCO")]
public class CastEntity
{
    [Column("id"), JsonPropertyName("id"), Key, Required,
     DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; set; }
    [Column("media_id"), JsonPropertyName("mediaId"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("cast_member"), JsonPropertyName("name"), Required, MaxLength(50)]
    public required string Name { get; init; }
    [JsonIgnore]
    public MediaEntity Media { get; init; } = null!;
}

public class CastEntityConfiguration : IEntityTypeConfiguration<CastEntity>
{
    public void Configure(EntityTypeBuilder<CastEntity> builder)
    {
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Cast)
            .HasForeignKey(e => e.MediaId);
    }
}