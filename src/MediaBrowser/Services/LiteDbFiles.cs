using LiteDB;
using MediaBrowser.Attributes;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class LiteDbFiles : IFiles
    {
        public LiteDbFiles(ILiteDatabase db)
        {
            Collection = db.GetCollection<LiteDbFile>("files");
            Database = db;
        }

        public ILiteCollection<LiteDbFile> Collection { get; }
        public ILiteDatabase Database { get; }

        public Task<IFile> Get(Guid fileId) =>
            Task.FromResult((IFile)Collection.FindById(fileId));

        public Task<IFile> GetByName(string name) =>
            Task.FromResult((IFile)Collection.FindOne(Query.EQ(nameof(LiteDbFile.Name), name)));

        public Task<SearchFilesResponse<IFile>> Search(SearchFilesRequest request, Guid userId, HashSet<string> userRoles)
        {
            var query = Query.All();

            query.Offset = request.Skip;
            query.Limit = request.Take;

            if (!userRoles.Contains(RequiresAdminRoleAttribute.AdminRole))
            {
                var accessFilter = new List<BsonExpression>
                {
                    Query.EQ(nameof(LiteDbFile.UploadedBy), userId)
                };

                foreach (var role in userRoles)
                {
                    accessFilter.Add(Query.Contains(nameof(LiteDbFile.ReadRoles), role));
                }

                query.Where.Add(Query.Or(accessFilter.ToArray()));
            }

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var keywordQuery = new List<BsonExpression>();

                foreach (var term in Regex.Split(request.Keywords, @"\s+"))
                {
                    keywordQuery.Add(Query.Contains(nameof(LiteDbFile.Description), term));
                    keywordQuery.Add(Query.Contains(nameof(LiteDbFile.Name), term));
                }

                if (keywordQuery.Count > 0)
                {
                    query.Where.Add(Query.Or(keywordQuery.ToArray()));
                }
            }

            switch (request.Filter)
            {
                case FileFilterOptions.AudioFiles:
                    query.Where.Add(Query.EQ(nameof(LiteDbFile.Type), FileType.Audio.ToString()));
                    break;
                case FileFilterOptions.Photos:
                    query.Where.Add(Query.EQ(nameof(LiteDbFile.Type), FileType.Photo.ToString()));
                    break;
                case FileFilterOptions.Videos:
                    query.Where.Add(Query.EQ(nameof(LiteDbFile.Type), FileType.Video.ToString()));
                    break;
            }

            switch (request.Sort)
            {
                case FileSortOptions.CreatedOn:
                    query.OrderBy = nameof(LiteDbFile.UploadedOn);
                    break;
                case FileSortOptions.Duration:
                    query.OrderBy = nameof(LiteDbFile.Duration);
                    break;
                case FileSortOptions.Name:
                    query.OrderBy = nameof(LiteDbFile.Name);
                    break;
                case FileSortOptions.Type:
                    query.OrderBy = nameof(LiteDbFile.Type);
                    break;
            }

            query.Order = request.Ascending ? Query.Ascending : Query.Descending;

            var results = Collection.Find(query).ToArray();

            var response = new SearchFilesResponse<IFile>(request, 0, results.Cast<IFile>());

            if (results.Length < request.Take)
            {
                response.Count = request.Skip + results.Length;
            }
            else
            {
                query.Offset = 0;
                query.Limit = int.MaxValue;
                response.Count = Collection.Count(query);
            }

            return Task.FromResult(response);
        }

        public Task<IFile> Update(Guid fileId, UpdateFileRequest request, IThumbnail[] thumbnails = null)
        {
            var file = Collection.FindById(fileId);

            if (file == null)
            {
                return Task.FromResult((IFile)null);
            }

            file.Name = request.Name ?? file.Name;
            file.Description = request.Description ?? file.Description;
            file.ReadRoles = request.ReadRoles ?? file.ReadRoles;
            file.Thumbnails = thumbnails ?? file.Thumbnails;
            file.UpdateRoles = request.UpdateRoles ?? file.UpdateRoles;

            Collection.Update(file.Id, file);

            return Task.FromResult((IFile)file);
        }

        public Task<IFile> Upload(UploadFileRequest request, UploadedFileInfo file)
        {
            var liteDbFile = new LiteDbFile
            {
                AudioStreams = file.AudioStreams,
                ContentLength = file.ContentLength,
                ContentType = file.ContentType,
                Description = request.Description ?? "",
                Duration = file.Duration,
                Fps = file.Fps,
                Height = file.Height,
                Id = file.Id,
                Name = request.Name ?? "",
                LiteDbThumbnails = file.Thumbnails?.Select(it => new LiteDbThumbnail(it)).ToArray(),
                Location = file.Location,
                Md5 = file.Md5,
                ReadRoles = request.ReadRoles ?? new HashSet<string>(),
                Type = file.Type,
                UpdateRoles = request.UpdateRoles ?? new HashSet<string>(),
                UploadedBy = file.UploadedBy,
                UploadedOn = file.UploadedOn,
                VideoStreams = file.VideoStreams,
                Width = file.Width
            };

            Collection.Insert(liteDbFile);

            return Task.FromResult((IFile)liteDbFile);
        }
    }

    public class LiteDbFile : IFile
    {
        public DateTime UploadedOn { get; set; }

        public FileType Type { get; set; }

        public Guid Id { get; set; }

        public Guid Md5 { get; set; }

        public Guid UploadedBy { get; set; }

        public HashSet<string> ReadRoles { get; set; }

        public HashSet<string> UpdateRoles { get; set; }

        [BsonField(nameof(Thumbnails))]
        public LiteDbThumbnail[] LiteDbThumbnails
        {
            get => Thumbnails?.Select(it => it as LiteDbThumbnail ?? new LiteDbThumbnail(it)).ToArray();
            set => Thumbnails = value;
        }

        [BsonIgnore]
        public IThumbnail[] Thumbnails { get; set; }

        public double? Fps { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }

        public long ContentLength { get; set; }

        public long? Duration { get; set; }

        public string ContentType { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string Name { get; set; }

        public string[] AudioStreams { get; set; }

        public string[] VideoStreams { get; set; }
    }

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
