namespace MediaBrowser.Media.Directors;

[Table("media_directors")]
public class DirectorEntity
{
    [Column("id"), JsonPropertyName("id"), Key, Required,
     DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; set; } 
    [Column("media_id"), JsonPropertyName("mediaId"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("director"), JsonPropertyName("name"), Required, MaxLength(50)]
    public required string Name { get; init; }
    [JsonPropertyName("media")]
    public MediaEntity Media { get; init; } = null!;
}

public class DirectorEntityConfiguration : IEntityTypeConfiguration<DirectorEntity>
{
    public void Configure(EntityTypeBuilder<DirectorEntity> builder)
    {
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Directors)
            .HasForeignKey(e => e.MediaId);
    }
}
