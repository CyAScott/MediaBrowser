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
        /// The location of ffmpeg.
        /// </summary>
        public string Ffmpeg { get; set; }

        /// <summary>
        /// The location of media files.
        /// </summary>
        public string MediaFiles { get; set; }

        /// <summary>
        /// The scratch disk directory.
        /// </summary>
        public string Temp { get; set; }
    }
}
