using Castle.Core;
using MediaBrowser.Attributes;
using MediaBrowser.Extensions;
using MediaBrowser.Models;
using MediaBrowser.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.CommandLine
{
    [CommandInfo("Imports media files into the DB.")]
    public class Import : CommandLineArgs, IAmACommand
    {
        private static readonly HashSet<string> defaultFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".m4a", ".m4p", ".m4b", ".m4r", ".m4v",
            ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi",
            ".gif",
            ".png",
            ".tiff", ".tif",
            ".mp3"
        };

        private async Task scanDirectory(string directory, HashSet<string> fileExtensions)
        {
            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                await scanDirectory(subDirectory, fileExtensions);
            }

            foreach (var location in Directory
                .GetFiles(directory, "*.*")
                .Where(file => fileExtensions.Contains(Path.GetExtension(file) ?? "")))
            {
                Guid md5;
                using (var reader = File.OpenRead(location))
                using (var md5Hasher = MD5.Create())
                {
                    md5 = new Guid(md5Hasher.ComputeHash(reader));
                }

                var files = await Files.GetByMd5(md5);
                if (files.Length > 0)
                {
                    Log.Warn($"Skipping file \"{location}\"");
                    continue;
                }

                UploadedFileInfo fileInfo;
                try
                {
                    fileInfo = await Ffmpeg.GetFileInfo(location);
                }
                catch (Exception error)
                {
                    Log.Error($"Unable to read file \"{location}\": {error}");
                    continue;
                }

                UploadedFileInfo thumbnail = null;
                var fileId = Guid.NewGuid();
                var tempLocations = new List<string>();
                var thumbnailInfo = new List<UploadedFileInfo>();
                var type = FileType.Photo;

                try
                {
                    if (fileInfo.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    {
                        type = FileType.Video;

                        var tempLocation = Path.Combine(Locations.Temp, Guid.NewGuid().ToString());
                        tempLocations.Add(tempLocation);

                        thumbnail = await Ffmpeg.GenerateThumbnail(location, new TimeSpan(TimeSpan.FromMilliseconds(fileInfo.Duration ?? 0).Ticks / 2), tempLocation);

                        var thumbnailLocation = Path.Combine(Locations.MediaFiles, $"{fileId}.{thumbnail.Md5}.jpg");

                        if (File.Exists(thumbnailLocation))
                        {
                            File.Delete(thumbnailLocation);
                        }

                        File.Move(thumbnail.Location, thumbnailLocation);

                        thumbnail.Location = thumbnailLocation;
                    }
                    else if (fileInfo.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                    {
                        type = FileType.Audio;
                    }
                }
                catch
                {
                    if (!string.IsNullOrEmpty(thumbnail?.Location) && File.Exists(thumbnail.Location))
                    {
                        File.Delete(thumbnail.Location);
                    }
                    throw;
                }
                finally
                {
                    foreach (var tempLocation in tempLocations)
                    {
                        if (!string.IsNullOrEmpty(tempLocation) && File.Exists(tempLocation))
                        {
                            File.Delete(tempLocation);
                        }
                    }
                }

                var file = await Files.Upload(new UploadFileRequest
                {
                    Description = "",
                    Name = Path.GetFileNameWithoutExtension(location),
                    ReadRoles = new HashSet<string>(),
                    UpdateRoles = new HashSet<string>()
                },
                new UploadedFileInfo
                {
                    AudioStreams = fileInfo.AudioStreams,
                    ContentLength = new FileInfo(location).Length,
                    ContentType = fileInfo.ContentType,
                    Fps = fileInfo.Fps,
                    Height = fileInfo.Height,
                    Id = fileId,
                    Location = location,
                    Md5 = md5,
                    Thumbnails = thumbnail == null ? new IThumbnail[0] : new IThumbnail[]
                    {
                        new Thumbnail(thumbnail)
                    },
                    Type = type,
                    UploadedOn = DateTime.UtcNow,
                    VideoStreams = fileInfo.VideoStreams,
                    Width = fileInfo.Width
                });
            }
        }

        public DiskLocations Locations { get; set; }

        public IFfmpeg Ffmpeg { get; set; }

        public IFiles Files { get; set; }

        public ILogger Log { get; set; }

        [CommandLineArgument("location", Description = "The directory to scan for media files."), DoNotWire]
        public string Location { get; set; }

        public Task Invoke(string[] args)
        {
            if (string.IsNullOrEmpty(Location) || !Directory.Exists(Location))
            {
                throw new ArgumentException("The location was not provided or does not exist.");
            }

            return scanDirectory(Location, defaultFileExtensions);
        }
    }
}
