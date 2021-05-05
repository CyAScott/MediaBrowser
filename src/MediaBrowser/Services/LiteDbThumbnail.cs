using MediaBrowser.Models;
using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class LiteDbThumbnail : IThumbnail
    {
        public LiteDbThumbnail()
        {
        }
        public LiteDbThumbnail(IThumbnail thumbnail)
        {
            ContentLength = thumbnail.ContentLength;
            ContentType = thumbnail.ContentType;
            CreatedOn = thumbnail.CreatedOn;
            Height = thumbnail.Height;
            Location = thumbnail.Location;
            Md5 = thumbnail.Md5;
            Width = thumbnail.Width;
        }

        public DateTime CreatedOn { get; set; }

        public Guid Md5 { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public long ContentLength { get; set; }

        public string Location { get; set; }

        public string ContentType { get; set; }
    }
}
