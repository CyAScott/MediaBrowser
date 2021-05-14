using Castle.Core;
using MediaBrowser.Attributes;
using MediaBrowser.Extensions;
using MediaBrowser.Models;
using MediaBrowser.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.CommandLine
{
    [CommandInfo("Imports media files into the DB.")]
    public class Import : CommandLineArgs, IAmACommand
    {
        private async Task scanDirectory(Guid userId, DateTime now, string directory, HashSet<string> fileExtensions, Dictionary<string, IPlaylist> playlists)
        {
            foreach (var subDirectory in Directory.GetDirectories(directory).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
            {
                await scanDirectory(userId, now, subDirectory, fileExtensions, playlists);
            }

            var playlistName = Format(PlaylistNameFormat, now, directory);

            foreach (var location in Directory
                .GetFiles(directory, "*.*")
                .Where(file => fileExtensions.Contains((Path.GetExtension(file) ?? "").TrimStart('.')))
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
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

                UploadedFileInfo uploadedFileInfo;
                try
                {
                    uploadedFileInfo = await Ffmpeg.GetFileInfo(location);
                }
                catch (Exception error)
                {
                    Log.Error($"Unable to read file \"{location}\": {error}");
                    continue;
                }

                GeneratedThumbnail thumbnail = null;
                var fileId = Guid.NewGuid();
                var fileInfo = new FileInfo(location);
                var thumbnailLocations = new List<string>();
                var thumbnailInfo = new List<UploadedFileInfo>();
                var type = FileType.Photo;

                try
                {
                    if (uploadedFileInfo.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    {
                        type = FileType.Video;

                        var thumbnailLocation = Path.Combine(Locations.MediaFiles, $"{fileId}.{Guid.NewGuid()}.jpg");
                        thumbnailLocations.Add(thumbnailLocation);

                        if (File.Exists(thumbnailLocation))
                        {
                            File.Delete(thumbnailLocation);
                        }

                        thumbnail = await Ffmpeg.GenerateThumbnail(location,
                            new TimeSpan(TimeSpan.FromMilliseconds(uploadedFileInfo.Duration ?? 0).Ticks / 2),
                            getThumbnailSize(uploadedFileInfo.Width ?? 0, uploadedFileInfo.Height ?? 0),
                            thumbnailLocation);

                        thumbnail.Location = Path.Combine(Locations.MediaFiles, $"{fileId}.{thumbnail.Md5}.jpg");
                        thumbnailLocations.Add(thumbnail.Location);

                        File.Move(thumbnailLocation, thumbnail.Location);
                    }
                    else if (uploadedFileInfo.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                    {
                        type = FileType.Audio;
                    }

                    var file = await Files.Upload(new UploadFileRequest
                        {
                            Description = Format(DescriptionFormat, now, location),
                            Name = Format(NameFormat, now, location),
                            ReadRoles = new RoleSet(ReadRoles ?? new string[0]),
                            UpdateRoles = new RoleSet(UpdateRoles ?? new string[0])
                        },
                        new UploadedFileInfo
                        {
                            AudioStreams = uploadedFileInfo.AudioStreams,
                            ContentLength = fileInfo.Length,
                            ContentType = uploadedFileInfo.ContentType,
                            Fps = uploadedFileInfo.Fps,
                            Height = uploadedFileInfo.Height,
                            Id = fileId,
                            Location = location,
                            Md5 = md5,
                            Thumbnails = thumbnail == null ? new IThumbnail[0] : new[] { thumbnail },
                            Type = type,
                            UploadedBy = userId,
                            UploadedOn = fileInfo.CreationTime,
                            VideoStreams = uploadedFileInfo.VideoStreams,
                            Width = uploadedFileInfo.Width
                        });

                    Log.Info($"Media file added: {file.Name}");

                    thumbnailLocations.Clear();

                    if (string.IsNullOrEmpty(playlistName))
                    {
                        continue;
                    }

                    if (!playlists.TryGetValue(playlistName, out var playlist))
                    {
                        Guid playlistId;
                        using (var md5Hasher = MD5.Create())
                        {
                            playlistId = new Guid(md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(playlistName.ToLower())));
                        }

                        playlist = await Playlists.Get(playlistId);

                        if (playlist == null)
                        {
                            if (thumbnail != null)
                            {
                                var thumbnailLocation = Path.Combine(Locations.MediaFiles, $"playlist.{playlistId}.{thumbnail.Md5}.jpg");
                                thumbnailLocations.Add(thumbnailLocation);

                                if (File.Exists(thumbnailLocation))
                                {
                                    File.Delete(thumbnailLocation);
                                }

                                File.Copy(thumbnail.Location, thumbnailLocation);

                                thumbnail.Location = thumbnailLocation;
                            }
                            else if (type == FileType.Photo)
                            {
                                var thumbnailLocation = Path.Combine(Locations.MediaFiles, $"playlist.{fileId}.{Guid.NewGuid()}.jpg");
                                thumbnailLocations.Add(thumbnailLocation);

                                if (File.Exists(thumbnailLocation))
                                {
                                    File.Delete(thumbnailLocation);
                                }

                                thumbnail = await Ffmpeg.GenerateThumbnail(location,
                                    TimeSpan.Zero,
                                    getThumbnailSize(uploadedFileInfo.Width ?? 0, uploadedFileInfo.Height ?? 0),
                                    thumbnailLocation);

                                thumbnail.Location = Path.Combine(Locations.MediaFiles, $"playlist.{fileId}.{thumbnail.Md5}.jpg");
                                thumbnailLocations.Add(thumbnail.Location);

                                File.Move(thumbnailLocation, thumbnail.Location);
                            }

                            playlist = await Playlists.Create(new CreatePlaylistRequest
                                {
                                    Description = Format(PlaylistDescriptionFormat, now, directory),
                                    Name = playlistName,
                                    ReadRoles = new RoleSet(ReadRoles ?? new string[0]),
                                    UpdateRoles = new RoleSet(UpdateRoles ?? new string[0])
                                },
                                playlistId,
                                userId,
                                thumbnail == null ? new IThumbnail[0] : new[] { thumbnail });

                            thumbnailLocations.Clear();


                            Log.Info($"Playlist added: {playlist.Name}");
                        }

                        playlists[playlistName] = playlist;
                    }

                    await Files.SetPlaylistReference(file.Id, playlist, -1);
                }
                catch
                {
                    foreach (var thumbnailLocation in thumbnailLocations)
                    {
                        if (!string.IsNullOrEmpty(thumbnailLocation) && File.Exists(thumbnailLocation))
                        {
                            File.Delete(thumbnailLocation);
                        }
                    }
                    throw;
                }
            }
        }

        private static Size getThumbnailSize(int width, int height) => height < 480 ? new Size(width, height) : new Size(-1, 480);

        private static readonly string[] defaultFileExtensions = new[]
        {
            ".mp4", ".m4a", ".m4p", ".m4b", ".m4r", ".m4v",
            ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi",
            ".gif",
            ".png",
            ".tiff", ".tif",
            ".mp3"
        };

        public DiskLocations Locations { get; set; }

        public IFfmpeg Ffmpeg { get; set; }

        public IFiles Files { get; set; }

        public ILogger Log { get; set; }

        public IPlaylists Playlists { get; set; }

        public IRoles Roles { get; set; }

        public IUsers Users { get; set; }

        [CommandLineArgument("description", Description = "The template for file descriptions."), DoNotWire]
        public string DescriptionFormat { get; set; }

        /// <summary>
        /// Formats a string based on a template and a directory/file location.
        /// </summary>
        public string Format(string format, DateTime now, string location)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            var locationInfo = File.Exists(location) ? (FileSystemInfo)new FileInfo(location) : new DirectoryInfo(location);

            var variables = new Dictionary<string, object>
            {
                { nameof(locationInfo.CreationTime), locationInfo.CreationTimeUtc },
                { nameof(FileInfo.DirectoryName), Path.GetFileName(Path.GetDirectoryName(location)) },
                { nameof(locationInfo.Extension), locationInfo.Extension?.Trim('.') ?? "" },
                { nameof(locationInfo.FullName), locationInfo.FullName },
                { nameof(locationInfo.LastAccessTime), locationInfo.LastAccessTimeUtc },
                { nameof(locationInfo.LastWriteTime), locationInfo.LastWriteTimeUtc },
                { nameof(FileInfo.Length), (locationInfo as FileInfo)?.Length ?? 0 },
                { nameof(locationInfo.Name), Path.GetFileNameWithoutExtension(location) },
            };

            var variablePattern = new Regex(@"(?<start>\{)(?<middle>[^\},:]+)(?<end>([:,][^\}]*)?\})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return String.Format(
                variablePattern.Replace(format, match => $"{match.Groups["start"].Value}{variables.Select((pair, index) => new { match = string.Equals(pair.Key, match.Groups["middle"].Value, StringComparison.OrdinalIgnoreCase), index }).First(it => it.match).index}{match.Groups["end"].Value}"),
                variables.Select(it => it.Value).ToArray());

        }

        [CommandLineArgument("location", Description = "The directory to scan for media files."), DoNotWire, Required]
        public string Location { get; set; }

        [CommandLineArgument("name", Description = "The template for file names."), DefaultValue("{name}"), DoNotWire, Required]
        public string NameFormat { get; set; } = "{name}";

        [CommandLineArgument("playlistName", Description = "The template for playlist names."), DefaultValue("{name}"), DoNotWire, Required]
        public string PlaylistNameFormat { get; set; } = "{name}";

        [CommandLineArgument("playlistDescription", Description = "The template for playlist descriptions."), DoNotWire]
        public string PlaylistDescriptionFormat { get; set; }

        [CommandLineArgument("uploadedBy", Description = "The user id or user name for the imported files and playlists."), DoNotWire, Required]
        public string UploadedBy { get; set; }

        [CommandLineArgument("fileExtensions", Description = "The file extensions to scan for."), DoNotWire]
        public string[] FileExtensions { get; set; }

        [CommandLineArgument("readRoles", Description = "The roles for reading the file and playlists"), DefaultValue(new[] { "ADMIN" }), DoNotWire]
        public string[] ReadRoles { get; set; } = new[] { "ADMIN" };

        [CommandLineArgument("updateRoles", Description = "The roles for updating the file and playlists"), DefaultValue(new[] { "ADMIN" }), DoNotWire]
        public string[] UpdateRoles { get; set; } = new[] { "ADMIN" };

        public async Task Invoke(string[] args)
        {
            if (string.IsNullOrEmpty(Location) || !Directory.Exists(Location))
            {
                throw new ArgumentException("The location was not provided or does not exist.");
            }

            if (Guid.TryParse(UploadedBy ?? "", out var userId))
            {
                if (await Users.Get(userId) == null)
                {
                    throw new ArgumentException("The user was not found.");
                }
            }
            else
            {
                var user = await Users.GetByUserName(UploadedBy ?? "");
                if (user == null)
                {
                    throw new ArgumentException("The user was not found.");
                }
                userId = user.Id;
            }

            if (!await Roles.DoRolesExists(new RoleSet(ReadRoles ?? new string[0]), new RoleSet(UpdateRoles ?? new string[0])))
            {
                throw new ArgumentException("Roles not found.");
            }

            await scanDirectory(userId,
                DateTime.UtcNow,
                Location,
                new HashSet<string>((FileExtensions ?? defaultFileExtensions).Select(it => it.TrimStart('.')), StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, IPlaylist>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
