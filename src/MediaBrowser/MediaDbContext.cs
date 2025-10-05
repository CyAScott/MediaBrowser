namespace MediaBrowser;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<CastEntity> Casts { get; init; }
    public DbSet<DirectorEntity> Directors { get; init; }
    public DbSet<GenreEntity> Genres { get; init; }
    public DbSet<MediaEntity> Media { get; init; }

    public IQueryable<MediaJoin> MediaJoin => Media
        .GroupJoin(Casts, it => it.Id, it => it.MediaId,
            (it, group) => new MediaJoin
            {
                Media = it,
                Cast = group.Select(x => x.Name).ToArray()
            })
        .GroupJoin(Directors, it => it.Media.Id, it => it.MediaId,
            (it, group) => new MediaJoin
            {
                Media = it.Media,
                Cast = it.Cast,
                Directors = group.Select(x => x.Name).ToArray()
            })
        .GroupJoin(Genres, it => it.Media.Id, it => it.MediaId,
            (it, group) => new MediaJoin
            {
                Media = it.Media,
                Cast = it.Cast,
                Directors = it.Directors,
                Genres = group.Select(x => x.Name).ToArray()
            })
        .GroupJoin(Producers, it => it.Media.Id, it => it.MediaId,
            (it, group) => new MediaJoin
            {
                Media = it.Media,
                Cast = it.Cast,
                Directors = it.Directors,
                Genres = it.Genres,
                Producers = group.Select(x => x.Name).ToArray()
            })
        .GroupJoin(Writers, it => it.Media.Id, it => it.MediaId,
            (it, group) => new MediaJoin
            {
                Media = it.Media,
                Cast = it.Cast,
                Directors = it.Directors,
                Genres = it.Genres,
                Producers = it.Producers,
                Writers = group.Select(x => x.Name).ToArray()
            });
    public DbSet<ProducerEntity> Producers { get; init; }
    public DbSet<WriterEntity> Writers { get; init; }
    public DbSet<UserEntity> Users { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CastEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DirectorEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MediaEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProducerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new WriterEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}