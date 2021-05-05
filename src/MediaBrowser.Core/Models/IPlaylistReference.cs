using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A reference to a playlist.
    /// </summary>
    public interface IPlaylistReference
    {
        /// <summary>
        /// When this media file was added to the playlist.
        /// </summary>
        DateTime AddedOn { get; }

        /// <summary>
        /// The playlist id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The index for this media file in the playlist.
        /// </summary>
        int Index { get; }
    }
}
