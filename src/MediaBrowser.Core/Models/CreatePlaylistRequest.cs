using System.Collections.Generic;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to create a playlist.
    /// </summary>
    public class CreatePlaylistRequest
    {
        /// <summary>
        /// The required roles for reading the playlist.
        /// </summary>
        public HashSet<string> ReadRoles { get; set; }

        /// <summary>
        /// The required roles for updating the playlist.
        /// </summary>
        public HashSet<string> UpdateRoles { get; set; }

        /// <summary>
        /// A friendly description for the playlist.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The playlist name.
        /// </summary>
        public string Name { get; set; }
    }
}
