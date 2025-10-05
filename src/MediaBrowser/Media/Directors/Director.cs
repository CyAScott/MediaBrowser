namespace MediaBrowser.Media.Directors;

public class DirectorEntity
{
    [Column("id"), Key]
    public required int Id { get; init; }
    [Column("media_id")]
    public required Guid MediaId { get; init; }
    [Column("director"), Required, MaxLength(50)]
    public required string Name { get; init; }
}

public class DirectorEntityConfiguration : IEntityTypeConfiguration<DirectorEntity>
{
    public void Configure(EntityTypeBuilder<DirectorEntity> builder)
    {
        builder.ToTable("media_directors");
        
        builder.HasIndex(e => new { e.MediaId, Director = e.Name })
            .IsUnique();

        builder.HasOne<MediaEntity>()
            .WithMany()
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
