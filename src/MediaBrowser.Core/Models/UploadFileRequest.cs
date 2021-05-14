using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to upload a media file.
    /// </summary>
    public class UploadFileRequest
    {
        /// <summary>
        /// The required roles for reading the file.
        /// </summary>
        public RoleSet ReadRoles { get; set; }

        /// <summary>
        /// The required roles for updating the file.
        /// </summary>
        public RoleSet UpdateRoles { get; set; }

        /// <summary>
        /// A friendly description for the file.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The file name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Information about an uploaded file.
    /// </summary>
    public class UploadedFileInfo
    {
        /// <summary>
        /// When the file was uploaded.
        /// </summary>
        public DateTime UploadedOn { get; set; }

        /// <summary>
        /// The type of media file.
        /// </summary>
        public FileType Type { get; set; }

        /// <summary>
        /// The thumbnails for the file.
        /// </summary>
        public IThumbnail[] Thumbnails { get; set; }

        /// <summary>
        /// The id for this file.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The md5 hash of the file contents.
        /// </summary>
        public Guid Md5 { get; set; }

        /// <summary>
        /// The user id for the person who uploaded the file.
        /// </summary>
        public Guid UploadedBy { get; set; }

        /// <summary>
        /// The file's FPS.
        /// </summary>
        public double? Fps { get; set; }

        /// <summary>
        /// The pixel height of the file.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// The pixel width of the file.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The duration in milliseconds of the file.
        /// </summary>
        public long? Duration { get; set; }

        /// <summary>
        /// The mime type of the file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The file location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The embedded audio streams in the file.
        /// </summary>
        public string[] AudioStreams { get; set; }

        /// <summary>
        /// The embedded video streams in the file.
        /// </summary>
        public string[] VideoStreams { get; set; }
    }
}
