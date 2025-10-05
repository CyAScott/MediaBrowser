namespace MediaBrowser;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<CastEntity> Casts { get; init; }
    public DbSet<DirectorEntity> Directors { get; init; }
    public DbSet<GenreEntity> Genres { get; init; }
    public DbSet<MediaEntity> Media { get; init; }
    public IQueryable<MediaEntity> MediaJoined => Media
        .Include(m => m.Cast)
        .Include(m => m.Directors)
        .Include(m => m.Genres)
        .Include(m => m.Producers)
        .Include(m => m.Writers);
    public DbSet<ProducerEntity> Producers { get; init; }
    public DbSet<WriterEntity> Writers { get; init; }
    public DbSet<UserEntity> Users { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CastEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DirectorEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProducerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new WriterEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

        modelBuilder.ApplyConfiguration(new MediaEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}