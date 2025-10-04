namespace MediaBrowser.Media.Directors;

public class DirectorEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("director"), Required, MaxLength(50)]
    public required string Director { get; init; }
}

public class DirectorEntityConfiguration : IEntityTypeConfiguration<DirectorEntity>
{
    public void Configure(EntityTypeBuilder<DirectorEntity> builder)
    {
        builder.ToTable("media_directors");
        
        builder.HasIndex(e => new { e.MediaId, e.Director })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
