using Microsoft.EntityFrameworkCore;

namespace MediaBrowser;

public class MediaSqliteDbContext(DbConfig config, DbContextOptions<MediaSqliteDbContext> options) : MediaDbContext(config.DbType, options);