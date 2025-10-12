namespace MediaBrowser.Media.Genres;

public class GenreEntity
{
    [Column("id"), Key, Required,
     DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; set; } 
    [Column("media_id"), MaxLength(36), Required]
    public required Guid MediaId { get; init; }
    [Column("genre"), Required, MaxLength(50)]
    public required string Name { get; init; }
    public MediaEntity Media { get; init; } = null!;
}

public class GenreEntityConfiguration : IEntityTypeConfiguration<GenreEntity>
{
    public void Configure(EntityTypeBuilder<GenreEntity> builder)
    {
        builder.ToTable("media_genres");
        
        builder.HasIndex(e => new { e.MediaId, e.Name }).IsUnique();

        builder.HasOne(e => e.Media)
            .WithMany(m => m.Genres)
            .HasForeignKey(e => e.MediaId);
    }
}