using System;
using System.Collections.Generic;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A media file.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// When the file was uploaded.
        /// </summary>
        DateTime UploadedOn { get; }

        /// <summary>
        /// The type of media file.
        /// </summary>
        FileType Type { get; }

        /// <summary>
        /// The id for this file.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The md5 hash of the file contents.
        /// </summary>
        Guid Md5 { get; }

        /// <summary>
        /// The user id for the person who uploaded the file.
        /// </summary>
        Guid UploadedBy { get; }

        /// <summary>
        /// The required roles for reading the file.
        /// </summary>
        HashSet<string> ReadRoles { get; }

        /// <summary>
        /// The required roles for updating the file.
        /// </summary>
        HashSet<string> UpdateRoles { get; }

        /// <summary>
        /// The thumbnails for the file.
        /// </summary>
        IThumbnail[] Thumbnails { get; }

        /// <summary>
        /// The file's FPS.
        /// </summary>
        double? Fps { get; }

        /// <summary>
        /// The pixel height of the file.
        /// </summary>
        int? Height { get; }

        /// <summary>
        /// The pixel width of the file.
        /// </summary>
        int? Width { get; }

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// The duration in milliseconds of the file.
        /// </summary>
        long? Duration { get; }

        /// <summary>
        /// The mime type of the file.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// A friendly description for the file.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The file location.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// The file name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The embedded audio streams in the file.
        /// </summary>
        string[] AudioStreams { get; }

        /// <summary>
        /// The embedded video streams in the file.
        /// </summary>
        string[] VideoStreams { get; }
    }

    /// <summary>
    /// File types.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// An audio file.
        /// </summary>
        Audio,

        /// <summary>
        /// A photo file.
        /// </summary>
        Photo,

        /// <summary>
        /// A video file.
        /// </summary>
        Video
    }
}
