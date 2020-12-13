using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Processes media file upload requests.
    /// </summary>
    public interface IUploadRequestProcessor
    {
        /// <summary>
        /// Processes an update media file request.
        /// </summary>
        Task<IFile> Process(Guid userId, IFile file, UpdateFileRequest request, List<FormFile> thumbnails = null);

        /// <summary>
        /// Processes an upload media file request.
        /// </summary>
        Task<IFile> Process(Guid userId, UploadFileRequest request, List<FormFile> files);
    }

    /// <summary>
    /// Processes media file upload requests.
    /// </summary>
    public class UploadRequestProcessor : IUploadRequestProcessor
    {
        private static readonly Dictionary<string, string>  fileExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "audio/mp3", "mp3" },
            { "audio/ogg", "oga" },
            { "audio/wav", "wav" },
            { "image/jpeg", "jpg" },
            { "image/gif", "gif" },
            { "image/png", "png" },
            { "image/bmp", "bmp" },
            { "image/tiff", "tif" },
            { "video/mp4", "mp4" },
            { "video/webm", "webm" },
            { "video/mpeg", "mpeg" },
            { "video/divx", "avi" },
            { "video/x-flv", "flv" },
            { "video/x-msVideo", "avi" },
            { "video/x-ms-wmv", "wmv" }
        };

        /// <inheritdoc/>
        public UploadRequestProcessor(DiskLocations locations, IFfmpeg ffmpeg, IFiles files)
        {
            Ffmpeg = ffmpeg;
            Files = files;
            Locations = locations;
        }

        /// <summary>
        /// A wrapper for ffmpeg.
        /// </summary>
        public IFfmpeg Ffmpeg { get; }

        /// <summary>
        /// The collection of media files.
        /// </summary>
        public IFiles Files { get; }

        /// <summary>
        /// Location info for storage.
        /// </summary>
        public DiskLocations Locations { get; }

        /// <inheritdoc/>
        public async Task<IFile> Process(Guid userId, IFile file, UpdateFileRequest request, List<FormFile> thumbnails = null)
        {
            var now = DateTime.UtcNow;
            var tempLocations = new List<string>();
            var thumbnailInfo = new List<UploadedFileInfo>();

            try
            {
                foreach (var uploadedFile in thumbnails ?? Enumerable.Empty<FormFile>())
                {
                    var tempLocation = Path.Combine(Locations.Temp, Guid.NewGuid().ToString());
                    tempLocations.Add(tempLocation);

                    using (var fileStream = File.Create(tempLocation))
                    {
                        await uploadedFile.CopyToAsync(fileStream, CancellationToken.None);
                    }

                    var fileInfo = await Ffmpeg.GetFileInfo(tempLocation);
                    fileInfo.UploadedBy = userId;
                    fileInfo.UploadedOn = now;

                    if (fileInfo.Type != FileType.Photo || !fileExtensions.ContainsKey(fileInfo.ContentType))
                    {
                        throw new UploadException(HttpStatusCode.ExpectationFailed, "Invalid Thumbnail File");
                    }

                    thumbnailInfo.Add(fileInfo);
                }

                var updatedThumbnails = new List<IThumbnail>();

                for (var index = 0; index < thumbnailInfo.Count; index++)
                {
                    var thumbnail = thumbnailInfo[index];

                    var newLocation = Path.Combine(Locations.MediaFiles, $"{file.Id}.{thumbnail.Md5}.{fileExtensions[thumbnail.ContentType]}");
                    tempLocations.Add(newLocation);
                    File.Move(thumbnail.Location, newLocation);
                    thumbnail.Location = newLocation;

                    updatedThumbnails.Add(new Thumbnail(thumbnail));
                }

                if (file.Thumbnails != null && file.Thumbnails.Length > 0)
                {
                    if (request.ThumbnailsToRemove != null)
                    {
                        foreach (var md5 in request.ThumbnailsToRemove)
                        {
                            var location = file.Thumbnails.FirstOrDefault(it => it.Md5 == md5)?.Location;
                            if (!string.IsNullOrEmpty(location) && File.Exists(location))
                            {
                                File.Delete(location);
                            }
                        }

                        updatedThumbnails.InsertRange(0, file.Thumbnails.Where(it => !request.ThumbnailsToRemove.Contains(it.Md5)));
                    }
                    else
                    {
                        updatedThumbnails.InsertRange(0, file.Thumbnails);
                    }
                }

                return await Files.Update(file.Id,  request, updatedThumbnails.ToArray());
            }
            catch (Exception)
            {
                foreach (var location in tempLocations)
                {
                    if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    {
                        File.Delete(location);
                    }
                }

                throw new UploadException(HttpStatusCode.InternalServerError, "Unknown Error");
            }
        }

        /// <inheritdoc/>
        public async Task<IFile> Process(Guid userId, UploadFileRequest request, List<FormFile> uploadedFiles)
        {
            var fileCount = uploadedFiles.Count(it => !it.IsThumbnail);

            if (fileCount == 0)
            {
                throw new UploadException(HttpStatusCode.ExpectationFailed, "Missing Media File");
            }

            if (fileCount > 1)
            {
                throw new UploadException(HttpStatusCode.ExpectationFailed, "Too Many Files Parts");
            }

            UploadedFileInfo mediaFileInfo = null;
            var now = DateTime.UtcNow;
            var tempLocations = new List<string>();
            var thumbnailInfo = new List<UploadedFileInfo>();

            try
            {
                foreach (var uploadedFile in uploadedFiles)
                {
                    var tempLocation = Path.Combine(Locations.Temp, Guid.NewGuid().ToString());
                    tempLocations.Add(tempLocation);

                    if (!uploadedFile.IsThumbnail)
                    {
                        request.Name = request.Name ?? Path.GetFileNameWithoutExtension(uploadedFile.FileName);
                    }

                    using (var fileStream = File.Create(tempLocation))
                    {
                        await uploadedFile.CopyToAsync(fileStream, CancellationToken.None);
                    }

                    var fileInfo = await Ffmpeg.GetFileInfo(tempLocation);
                    fileInfo.UploadedBy = userId;
                    fileInfo.UploadedOn = now;

                    if (!uploadedFile.IsThumbnail)
                    {
                        if (!fileExtensions.ContainsKey(fileInfo.ContentType))
                        {
                            throw new UploadException(HttpStatusCode.ExpectationFailed, "Invalid Media File");
                        }

                        mediaFileInfo = fileInfo;
                    }
                    else if (fileInfo.Type == FileType.Photo && fileExtensions.ContainsKey(fileInfo.ContentType))
                    {
                        thumbnailInfo.Add(fileInfo);
                    }
                    else
                    {
                        throw new UploadException(HttpStatusCode.ExpectationFailed, "Invalid Thumbnail File");
                    }
                }

                if (mediaFileInfo == null)
                {
                    throw new UploadException(HttpStatusCode.ExpectationFailed, "Missing Media File");
                }

                var newLocation = Path.Combine(Locations.MediaFiles, $"{mediaFileInfo.Id}.{fileExtensions[mediaFileInfo.ContentType]}");
                tempLocations.Add(newLocation);
                File.Move(mediaFileInfo.Location, newLocation);
                mediaFileInfo.Location = newLocation;

                for (var index = 0; index < thumbnailInfo.Count; index++)
                {
                    var thumbnail = thumbnailInfo[index];

                    newLocation = Path.Combine(Locations.MediaFiles, $"{mediaFileInfo.Id}.{thumbnail.Md5}.{fileExtensions[thumbnail.ContentType]}");
                    tempLocations.Add(newLocation);
                    File.Move(thumbnail.Location, newLocation);
                    thumbnail.Location = newLocation;
                }

                mediaFileInfo.Thumbnails = thumbnailInfo.Select(it => (IThumbnail)new Thumbnail(it)).ToArray();

                return await Files.Upload(request, mediaFileInfo);
            }
            catch (Exception)
            {
                foreach (var location in tempLocations)
                {
                    if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    {
                        File.Delete(location);
                    }
                }

                throw new UploadException(HttpStatusCode.InternalServerError, "Unknown Error");
            }
        }
    }

    /// <summary>
    /// An uploaded file.
    /// </summary>
    public class FormFile
    {
        /// <inheritdoc/>
        public FormFile(
            Func<Stream, CancellationToken, Task> copyToAsync,
            bool isThumbnail,
            string contentType,
            string fileName,
            string name,
            long length)
        {
            CopyToAsync = copyToAsync ?? throw new ArgumentNullException(nameof(copyToAsync));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            IsThumbnail = isThumbnail;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Length = length;
        }

        /// <summary>
        /// synchronously copies the contents of the uploaded file to the target stream.
        /// </summary>
        public Func<Stream, CancellationToken, Task> CopyToAsync { get; }

        /// <summary>
        /// If the file is a thumbnail.
        /// </summary>
        public bool IsThumbnail { get; }

        /// <summary>
        /// The mime type of the file.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The form name of the file.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of bytes in the file.
        /// </summary>
        public long Length { get; }
    }

    /// <inheritdoc/>
    public class Thumbnail : IThumbnail
    {
        /// <inheritdoc/>
        public Thumbnail(UploadedFileInfo info)
        {
            ContentLength = info.ContentLength;
            ContentType = info.ContentType;
            CreatedOn = info.UploadedOn;
            Location = info.Location;
            Md5 = info.Md5;
            Height = info.Height ?? 0;
            Width = info.Width ?? 0;
        }

        /// <inheritdoc/>
        public DateTime CreatedOn { get; }

        /// <inheritdoc/>
        public Guid Md5 { get; }

        /// <inheritdoc/>
        public int Height { get; }

        /// <inheritdoc/>
        public int Width { get; }

        /// <inheritdoc/>
        public long ContentLength { get; }

        /// <inheritdoc/>
        public string Location { get; }

        /// <inheritdoc/>
        public string ContentType { get; }
    }

    /// <summary>
    /// An exception when attempting to upload a file.
    /// </summary>
    public class UploadException : Exception
    {
        /// <inheritdoc/>
        public UploadException(HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// The http status code for the error.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }
    }
}
