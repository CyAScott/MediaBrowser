using Microsoft.EntityFrameworkCore;

namespace MediaBrowser;

public class MediaMySqlDbContext(DbConfig config, DbContextOptions<MediaMySqlDbContext> options) : MediaDbContext(config.DbType, options);