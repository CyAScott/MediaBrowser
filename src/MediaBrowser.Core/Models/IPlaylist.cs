using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A playlist file.
    /// </summary>
    public interface IPlaylist
    {
        /// <summary>
        /// When the playlist was created.
        /// </summary>
        DateTime CreatedOn { get; }

        /// <summary>
        /// The id for this playlist.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The user id for the person who created the playlist.
        /// </summary>
        Guid CreatedBy { get; }

        /// <summary>
        /// The required roles for reading the playlist.
        /// </summary>
        RoleSet ReadRoles { get; }

        /// <summary>
        /// The required roles for updating the playlist.
        /// </summary>
        RoleSet UpdateRoles { get; }

        /// <summary>
        /// The thumbnails for the playlist.
        /// </summary>
        IThumbnail[] Thumbnails { get; }

        /// <summary>
        /// A friendly description for the playlist.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The playlist name.
        /// </summary>
        string Name { get; }
    }
}
