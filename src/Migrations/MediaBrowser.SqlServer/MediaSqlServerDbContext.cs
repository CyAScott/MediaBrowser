using Microsoft.EntityFrameworkCore;

namespace MediaBrowser;

public class MediaSqlServerDbContext(DbConfig config, DbContextOptions<MediaSqlServerDbContext> options) : MediaDbContext(config.DbType, options);