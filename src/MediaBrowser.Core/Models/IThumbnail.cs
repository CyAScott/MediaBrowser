using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A thumbnail for a media file.
    /// </summary>
    public interface IThumbnail
    {
        /// <summary>
        /// When the file was uploaded.
        /// </summary>
        DateTime CreatedOn { get; }

        /// <summary>
        /// The md5 hash of the file contents.
        /// </summary>
        Guid Md5 { get; }

        /// <summary>
        /// The pixel height of the file.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The pixel width of the file.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// The file location.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// The mime type of the file.
        /// </summary>
        string ContentType { get; }
    }
}
