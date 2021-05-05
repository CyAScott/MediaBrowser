using System;
using System.Collections.Generic;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to update a playlist.
    /// </summary>
    public class UpdatePlaylistRequest
    {
        /// <summary>
        /// Optional. Thumbnails to remove by md5 hash.
        /// </summary>
        public HashSet<Guid> ThumbnailsToRemove { get; set; }

        /// <summary>
        /// Optional. If provided changes the required roles for reading the playlist.
        /// </summary>
        public HashSet<string> ReadRoles { get; set; }

        /// <summary>
        /// Optional. If provided changes the required roles for updating the playlist.
        /// </summary>
        public HashSet<string> UpdateRoles { get; set; }

        /// <summary>
        /// Optional. If provided changes the playlist name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional. If provided changes the friendly description for the playlist.
        /// </summary>
        public string Description { get; set; }
    }
}
