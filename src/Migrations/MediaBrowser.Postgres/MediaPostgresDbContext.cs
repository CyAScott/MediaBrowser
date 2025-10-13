using Microsoft.EntityFrameworkCore;

namespace MediaBrowser;

public class MediaPostgresDbContext(DbConfig config, DbContextOptions<MediaPostgresDbContext> options) : MediaDbContext(config.DbType, options);