using MediaBrowser.Attributes;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Contains configuration settings for database storage.
    /// </summary>
    [Configuration("Database")]
    public class DatabaseConfiguration
    {
        /// <summary>
        /// The database connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The database connection string type (i.e. LiteDb).
        /// </summary>
        public string ConnectionStringType { get; set; }
    }
}
