using MediaBrowser.Attributes;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Contains configuration settings for disk storage.
    /// </summary>
    [Configuration("Disk")]
    public class DiskLocations
    {
        /// <summary>
        /// The scratch disk directory.
        /// </summary>
        public string Temp { get; set; }
    }
}
