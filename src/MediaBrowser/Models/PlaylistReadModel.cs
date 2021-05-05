using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A read model for a playlist.
    /// </summary>
    public class PlaylistReadModel
    {
        private readonly IPlaylist playlist;

        /// <inheritdoc/>
        public PlaylistReadModel(IPlaylist playlist)
        {
            this.playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));

            Thumbnails = playlist.Thumbnails?.Select(it => new PlaylistThumbnailReadModel(playlist, it)).ToArray();
        }

        /// <inheritdoc/>
        public DateTime CreatedOn => playlist.CreatedOn;

        /// <inheritdoc/>
        public Guid Id => playlist.Id;

        /// <inheritdoc/>
        public Guid CreatedBy => playlist.CreatedBy;

        /// <inheritdoc/>
        public HashSet<string> ReadRoles => playlist.ReadRoles;

        /// <inheritdoc/>
        public HashSet<string> UpdateRoles => playlist.UpdateRoles;

        /// <inheritdoc/>
        public PlaylistThumbnailReadModel[] Thumbnails { get; }

        /// <inheritdoc/>
        public string Description => playlist.Description;

        /// <inheritdoc/>
        public string Name => playlist.Name;
    }

    /// <summary>
    /// A thumbnail for a playlist.
    /// </summary>
    public class PlaylistThumbnailReadModel
    {
        private readonly IPlaylist playlist;
        private readonly IThumbnail thumbnail;

        /// <inheritdoc/>
        public PlaylistThumbnailReadModel(IPlaylist playlist, IThumbnail thumbnail)
        {
            this.playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
            this.thumbnail = thumbnail ?? throw new ArgumentNullException(nameof(thumbnail));
        }

        /// <summary>
        /// When the file was uploaded.
        /// </summary>
        public DateTime CreatedOn => thumbnail.CreatedOn;

        /// <summary>
        /// The md5 hash of the file contents.
        /// </summary>
        public Guid Md5 => thumbnail.Md5;

        /// <summary>
        /// The pixel height of the file.
        /// </summary>
        public int Height => thumbnail.Height;

        /// <summary>
        /// The pixel width of the file.
        /// </summary>
        public int Width => thumbnail.Width;

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public long ContentLength => thumbnail.ContentLength;

        /// <summary>
        /// The mime type of the file.
        /// </summary>
        public string ContentType => thumbnail.ContentType;

        /// <summary>
        /// The url to read the file.
        /// </summary>
        public string Url => $"/api/playlists/{playlist.Id}/thumbnails/{thumbnail.Md5}/contents";
    }
}
