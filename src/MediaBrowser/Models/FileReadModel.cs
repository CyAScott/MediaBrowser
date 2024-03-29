using System;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A read model for a media file.
    /// </summary>
    public class FileReadModel
    {
        private readonly IFile file;

        /// <inheritdoc/>
        public FileReadModel(IFile file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));

            PlaylistReferences = file.PlaylistReferences?.Select(it => new PlaylistReferenceReadModel(it)).ToArray();
            Thumbnails = file.Thumbnails?.Select(it => new FileThumbnailReadModel(file, it)).ToArray();
        }

        /// <summary>
        /// The type of media file.
        /// </summary>
        public FileType Type => file.Type;

        /// <inheritdoc/>
        public DateTime UploadedOn => file.UploadedOn;

        /// <inheritdoc/>
        public Guid Id => file.Id;

        /// <inheritdoc/>
        public Guid UploadedBy => file.UploadedBy;

        /// <inheritdoc/>
        public RoleSet ReadRoles => file.ReadRoles;

        /// <inheritdoc/>
        public RoleSet UpdateRoles => file.UpdateRoles;

        /// <inheritdoc/>
        public Guid Md5 => file.Md5;

        /// <inheritdoc/>
        public FileThumbnailReadModel[] Thumbnails { get; }

        /// <inheritdoc/>
        public PlaylistReferenceReadModel[] PlaylistReferences { get; }

        /// <inheritdoc/>
        public double? Fps => file.Fps;

        /// <inheritdoc/>
        public int? Height => file.Height;

        /// <inheritdoc/>
        public int? Width => file.Width;

        /// <inheritdoc/>
        public long ContentLength => file.ContentLength;

        /// <inheritdoc/>
        public long? Duration => file.Duration;

        /// <inheritdoc/>
        public string ContentType => file.ContentType;

        /// <inheritdoc/>
        public string Description => file.Description;

        /// <inheritdoc/>
        public string Name => file.Name;

        /// <summary>
        /// The url to read the file.
        /// </summary>
        public string Url => $"/api/files/{file.Id}/contents";

        /// <inheritdoc/>
        public string[] AudioStreams => file.AudioStreams;

        /// <inheritdoc/>
        public string[] VideoStreams => file.VideoStreams;
    }

    /// <summary>
    /// A reference to a playlist for a media file.
    /// </summary>
    public class PlaylistReferenceReadModel
    {
        private readonly IPlaylistReference playlistReference;

        /// <inheritdoc/>
        public PlaylistReferenceReadModel(IPlaylistReference playlistReference) => this.playlistReference = playlistReference ?? throw new ArgumentNullException(nameof(playlistReference));

        /// <summary>
        /// When this media file was added to the playlist.
        /// </summary>
        public DateTime AddedOn => playlistReference.AddedOn;

        /// <summary>
        /// The playlist id.
        /// </summary>
        public Guid Id => playlistReference.Id;

        /// <summary>
        /// The index for this media file in the playlist.
        /// </summary>
        public int Index => playlistReference.Index;
    }

    /// <summary>
    /// A thumbnail for a media file.
    /// </summary>
    public class FileThumbnailReadModel
    {
        private readonly IFile file;
        private readonly IThumbnail thumbnail;

        /// <inheritdoc/>
        public FileThumbnailReadModel(IFile file, IThumbnail thumbnail)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
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
        public string Url => $"/api/files/{file.Id}/thumbnails/{thumbnail.Md5}/contents";
    }
}
