using MediaBrowser.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// A wrapper for ffmpeg.
    /// </summary>
    public interface IFfmpeg
    {
        /// <summary>
        /// Generates a thumbnails from a video file.
        /// </summary>
        Task<UploadedFileInfo> GenerateThumbnail(string location, TimeSpan offset, string thumbnailLocation);

        /// <summary>
        /// Reads the file meta data using ffmpeg.
        /// </summary>
        Task<UploadedFileInfo> GetFileInfo(string location);
    }

    /// <summary>
    /// A wrapper for ffmpeg.
    /// </summary>
    public class Ffmpeg : ConcurrentDictionary<int, Process>, IFfmpeg, IHaveInit
    {
        Task IHaveInit.Init()
        {
            if (!string.IsNullOrEmpty(Location) && File.Exists(Location))
            {
                return Task.CompletedTask;
            }

            //todo: download ffmpeg from cdn

            Location = "ffmpeg";

            return Task.CompletedTask;
        }

        private (Size? size, TimeSpan? duration, double? fps, string mime, string[] audioStreams, string[] videoStreams) probe(IEnumerable<string> lines)
        {
            TimeSpan? duration = null;
            var audioStreams = new List<(string line, HashSet<string> items)>();
            var videoStreams = new List<(string line, HashSet<string> items)>();

            foreach (var lineInfo in lines
                .Select(line => line.Trim())
                .SkipWhile(line => !line.StartsWith("Input #0"))
                .TakeWhile((line, index) => index == 0 || !line.StartsWith("Input #"))
                .Select(line => new
                {
                    line,
                    match = Regex.Match(line, @"^(Duration\s*:\s*\d+:\d+:\d+(\.\d*)?\s*,|Stream\s*#\d+:\d+((\(\w+\))?|(\[0x[\da-f]*\])?)\s*:\s*(?<type>(Video|Audio))\s*:)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
                })
                .Where(lineInfo => lineInfo.match.Success))
            {
                if (lineInfo.match.Value.StartsWith("Duration", StringComparison.OrdinalIgnoreCase))
                {
                    var line = lineInfo.match.Value;
                    var colonIndex = line.IndexOf(':');
                    duration = TimeSpan.TryParse(line.Substring(colonIndex + 1, line.IndexOf(',') - colonIndex - 1).Trim(), out var length) ? (TimeSpan?)length : null;
                }
                else if (lineInfo.match.Value.StartsWith("Stream", StringComparison.OrdinalIgnoreCase))
                {
                    var streamType = lineInfo.match.Groups["type"].Value;
                    var streamInfo = new HashSet<string>(lineInfo.line
                        .Substring(lineInfo.match.Value.Length + 1)
                        .Split(',')
                        .Select(item => item.Trim()), StringComparer.OrdinalIgnoreCase);

                    if (string.Equals(streamType, "Video", StringComparison.OrdinalIgnoreCase))
                    {
                        videoStreams.Add((lineInfo.line, streamInfo));
                    }
                    else if (string.Equals(streamType, "Audio", StringComparison.OrdinalIgnoreCase))
                    {
                        audioStreams.Add((lineInfo.line, streamInfo));
                    }
                }
            }

            Size? size;
            try
            {
                size = videoStreams
                    .Select(stream => stream.items
                        .Select(item => Regex.Match(item, @"^(\d+)x(\d+)(\s+\[.+\])?$", RegexOptions.Compiled))
                        .SingleOrDefault(match => match.Success))
                    .Where(match => match != null)
                    .Select(match => (Size?)new Size(Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value)))
                    .FirstOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Not a valid image/video file.");
            }

            double? fps;
            try
            {
                fps = videoStreams
                    .Select(stream => stream.items
                        .Select(item => Regex.Match(item, @"^\s*(?<fps>\d+(\.\d*)?)\s+fps\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
                        .SingleOrDefault(match => match.Success))
                    .Where(match => match != null)
                    .Select(match => Convert.ToDouble(match.Groups["fps"].Value.TrimEnd('.')))
                    .FirstOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Not a valid image/video file.");
            }

            if (audioStreams.Count == 0 && videoStreams.Count == 0)
            {
                throw new Exception("Not a valid media file.");
            }

            var mime = getImageMime(size, duration, audioStreams, videoStreams) ??
                       getVideoMime(size, audioStreams, videoStreams) ??
                       getAudioMime(size, audioStreams, videoStreams);

            if (string.IsNullOrEmpty(mime))
            {
                if (videoStreams.Count > 0)
                {
                    throw new Exception("Invalid video or photo file.");
                }

                if (audioStreams.Count > 0)
                {
                    throw new Exception("Invalid audio file.");
                }

                throw new Exception("Unable to detect the mime type.");
            }

            //sometimes ffmpeg thinks images have a duration
            if (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase) && mime != "image/gif")
            {
                duration = null;
            }

            return (size, duration, fps, mime,
                audioStreams.Select(stream => stream.line).ToArray(),
                videoStreams.Select(stream => stream.line).ToArray());
        }
        private string getAudioMime(Size? size, List<(string line, HashSet<string> items)> audioStreams, List<(string line, HashSet<string> items)> videoStreams)
        {
            if (size != null || audioStreams.Count == 0 || videoStreams.Count > 0)
            {
                return null;
            }

            if (audioStreams.Any(stream => stream.items.Any(item => item.StartsWith("mp3", StringComparison.OrdinalIgnoreCase))))
            {
                return "audio/mp3";
            }

            if (audioStreams.Any(stream => stream.items.Any(item => item.StartsWith("vorbis", StringComparison.OrdinalIgnoreCase))))
            {
                return "audio/ogg";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (audioStreams.Any(stream => stream.items.Any(item => item.StartsWith("pcm_", StringComparison.OrdinalIgnoreCase))))
            {
                return "audio/wav";
            }

            return null;
        }
        private string getImageMime(Size? size, TimeSpan? duration, List<(string line, HashSet<string> items)> audioStreams, List<(string line, HashSet<string> items)> videoStreams)
        {
            if (size == null || audioStreams.Count > 0 || videoStreams.Count != 1)
            {
                return null;
            }

            var metaData = videoStreams[0].items;

            if (duration != null)
            {
                if (metaData.Contains("mJpeg"))
                {
                    return "image/jpeg";
                }

                if (metaData.Contains("gif"))
                {
                    return "image/gif";
                }
            }
            else if (metaData.Contains("mJpeg"))
            {
                return "image/jpeg";
            }
            else if (metaData.Contains("gif"))
            {
                return "image/gif";
            }
            else if (metaData.Contains("png"))
            {
                return "image/png";
            }
            else if (metaData.Contains("bmp"))
            {
                return "image/bmp";
            }
            else if (metaData.Contains("tiff"))
            {
                return "image/tiff";
            }

            return null;
        }
        private string getVideoMime(Size? size, List<(string line, HashSet<string> items)> audioStreams, List<(string line, HashSet<string> items)> videoStreams)
        {
            if (size == null || videoStreams.Count == 0)
            {
                return null;
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("h264", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/mp4";
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("vp8", StringComparison.OrdinalIgnoreCase) ||
                                                                    item.StartsWith("vp9", StringComparison.OrdinalIgnoreCase))) &&
                audioStreams.Any(stream => stream.items.Any(item => item.StartsWith("vorbis", StringComparison.OrdinalIgnoreCase) ||
                                                                    item.StartsWith("opus", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/webm";
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("mpeg", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/mpeg";
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("msmpeg4v3", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/divx";
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("vp6f", StringComparison.OrdinalIgnoreCase) ||
                                                                    item.StartsWith("flv", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/x-flv";
            }

            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("rawVideo", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/x-msVideo";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (videoStreams.Any(stream => stream.items.Any(item => item.StartsWith("wmv", StringComparison.OrdinalIgnoreCase))))
            {
                return "video/x-ms-wmv";
            }

            return null;
        }
        private async Task<string[]> readLines(string arguments, Func<Stream, Task> stdIn = null, Stream stdOut = null)
        {
            var lines = new List<string>();

            //lets create a cancel token to kill ffmpeg if it doesn't finish in 2 minutes
            //2 minutes should be more than enough time for all assets
            using (var cancel = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
            using (var ffmpeg = new Process
            {
                StartInfo = new ProcessStartInfo(Location)
                {
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Maximized
                },
                EnableRaisingEvents = true
            })
            {
                var exited = new TaskCompletionSource<bool>();
                var readStdError = new TaskCompletionSource<bool>();

                ffmpeg.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data == null)
                    {
                        readStdError.TrySetResult(true);
                        return;
                    }

                    Debug.WriteLine(args.Data);

                    lines.Add(args.Data);
                };
                ffmpeg.Exited += (sender, args) => exited.TrySetResult(true);

                var id = -1;

                ffmpeg.Start();

                try
                {
                    cancel.Token.Register(ffmpeg.Kill);

                    this[id = ffmpeg.Id] = ffmpeg;

                    ffmpeg.BeginErrorReadLine();

                    var writeStdOut = stdOut == null ? Task.CompletedTask : ffmpeg.StandardOutput.BaseStream.CopyToAsync(stdOut);

                    if (stdIn != null)
                    {
                        try
                        {
                            await stdIn(ffmpeg.StandardInput.BaseStream);
                            ffmpeg.StandardInput.BaseStream.Close();
                        }
                        catch (IOException error) when (error.Message == "The pipe has been ended")
                        {
                            //this happens because sometimes ffmpeg only needs to read the file header
                            //so it will end the process before writing to std in is done
                        }
                    }

                    await Task.WhenAll(exited.Task, readStdError.Task, writeStdOut);
                }
                finally
                {
                    TryRemove(id, out _);

                    if (!exited.Task.IsCompleted)
                    {
                        try
                        {
                            ffmpeg.Kill();
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }
                    }
                }
            }

            return lines.ToArray();
        }

        /// <inheritdoc />
        public Ffmpeg(DiskLocations locations) => Location = locations.Ffmpeg;

        /// <summary>
        /// The ffmpeg location.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Reads a file.
        /// </summary>
        public async Task<UploadedFileInfo> GetFileInfo(string location)
        {
            var info = new FileInfo(location);
            var returnValue = new UploadedFileInfo
            {
                ContentLength = (int)info.Length,
                Id = Guid.NewGuid(),
                Location = location,
                UploadedOn = info.CreationTimeUtc
            };

            using (var md5 = MD5.Create())
            using (var reader = info.OpenRead())
            {
                returnValue.Md5 = new Guid(md5.ComputeHash(reader));
            }

            var lines = await readLines($"-i \"{location}\"");

            if (lines.Any(it => it.Contains("Invalid data found when processing input")))
            {
                File.Move(location, location + ".unknown");

                throw new Exception("Invalid data found when processing input");
            }

            var ffmpegInfo = probe(lines);

            returnValue.AudioStreams = ffmpegInfo.audioStreams;
            returnValue.ContentType = ffmpegInfo.mime;
            returnValue.Duration = ffmpegInfo.duration == null ? (long?)null : Convert.ToInt64(ffmpegInfo.duration.Value.TotalMilliseconds);
            returnValue.Fps = ffmpegInfo.fps;
            returnValue.Height = ffmpegInfo.size?.Height;
            returnValue.VideoStreams = ffmpegInfo.videoStreams;
            returnValue.Width = ffmpegInfo.size?.Width;

            if (ffmpegInfo.mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                returnValue.Type = FileType.Audio;
            }
            else if (ffmpegInfo.mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                returnValue.Type = FileType.Photo;
            }
            else if (ffmpegInfo.mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                returnValue.Type = FileType.Video;
            }

            return returnValue;
        }

        /// <summary>
        /// Generates a thumbnails from a video file.
        /// </summary>
        public async Task<UploadedFileInfo> GenerateThumbnail(string location, TimeSpan offset, string thumbnailLocation)
        {
            if (File.Exists(thumbnailLocation))
            {
                throw new DuplicateNameException();
            }

            await readLines($"-ss {Convert.ToInt32(Math.Floor(offset.TotalHours))}:{offset.Minutes:00}:{offset.Seconds:00}.{offset.Milliseconds:000} -i \"{location}\" -f mjpeg -vframes 1 \"{thumbnailLocation}\"");

            if (!File.Exists(thumbnailLocation))
            {
                throw new FileNotFoundException("The thumbnail is missing.");
            }

            return await GetFileInfo(thumbnailLocation);
        }
    }
}
