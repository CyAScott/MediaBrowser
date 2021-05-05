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
            BsonCollection = db.GetCollection<BsonDocument>("files");
            Collection = db.GetCollection<LiteDbFile>("files");
            Database = db;
        }

        public ILiteCollection<BsonDocument> BsonCollection { get; }
        public ILiteCollection<LiteDbFile> Collection { get; }
        public ILiteDatabase Database { get; }

        public Task<IFile> Get(Guid fileId) =>
            Task.FromResult((IFile)Collection.FindById(fileId));

        public Task<IFile> GetByName(string name) =>
            Task.FromResult((IFile)Collection.FindOne(Query.EQ(nameof(IFile.Name), name)));

        public Task<SearchFilesResponse<IFile>> Search(SearchFilesRequest request, Guid userId, HashSet<string> userRoles, Guid? playlistId = null)
        {
            var query = Query.All();

            query.Offset = request.Skip;
            query.Limit = request.Take;

            if (!userRoles.Contains(RequiresAdminRoleAttribute.AdminRole))
            {
                var accessFilter = new List<BsonExpression>
                {
                    Query.EQ(nameof(IFile.UploadedBy), userId)
                };

                foreach (var role in userRoles)
                {
                    accessFilter.Add(Query.EQ($"{nameof(IFile.ReadRoles)}[*] ANY", role));
                }

                query.Where.Add(Query.Or(accessFilter.ToArray()));
            }

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var keywordQuery = new List<BsonExpression>();

                foreach (var term in Regex.Split(request.Keywords, @"\s+"))
                {
                    keywordQuery.Add(Query.Contains(nameof(IFile.Description), term));
                    keywordQuery.Add(Query.Contains(nameof(IFile.Name), term));
                }

                if (keywordQuery.Count > 0)
                {
                    query.Where.Add(Query.Or(keywordQuery.ToArray()));
                }
            }

            switch (request.Filter)
            {
                case FileFilterOptions.AudioFiles:
                    query.Where.Add(Query.EQ(nameof(IFile.Type), FileType.Audio.ToString()));
                    break;
                case FileFilterOptions.Photos:
                    query.Where.Add(Query.EQ(nameof(IFile.Type), FileType.Photo.ToString()));
                    break;
                case FileFilterOptions.Videos:
                    query.Where.Add(Query.EQ(nameof(IFile.Type), FileType.Video.ToString()));
                    break;
            }

            if (playlistId != null)
            {
                query.Where.Add(Query.EQ($"{nameof(IFile.PlaylistReferences)}.Ids[*] ANY", playlistId.Value));
                query.OrderBy = $"{nameof(IFile.PlaylistReferences)}.Values.PL_{playlistId.Value.ToString().Replace('-', '_')}.{nameof(IPlaylistReference.Index)}";
            }
            else
            {
                switch (request.Sort)
                {
                    case FileSortOptions.CreatedOn:
                        query.OrderBy = nameof(IFile.UploadedOn);
                        break;
                    case FileSortOptions.Duration:
                        query.OrderBy = nameof(IFile.Duration);
                        break;
                    case FileSortOptions.Name:
                        query.OrderBy = nameof(IFile.Name);
                        break;
                    case FileSortOptions.Type:
                        query.OrderBy = nameof(IFile.Type);
                        break;
                }
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

        public Task<IFile> SetPlaylistReference(Guid fileId, IPlaylist playlist, int? index = null)
        {
            var idPath = $"{nameof(IFile.PlaylistReferences)}.Ids[*] ANY";
            var successful = true;

            void update(int oldIndex, int newIndex)
            {
                var plFieldName = $"PL_{playlist.Id.ToString().Replace('-', '_')}";

                IEnumerable<BsonDocument> transformDocs()
                {
                    var moveUp = newIndex > oldIndex;

                    using (var reader = Collection
                        .Query()
                        .Where(Query.And(
                            Query.Not("_id", fileId),
                            Query.EQ(idPath, playlist.Id),
                            Query.Between($"$.{nameof(IFile.PlaylistReferences)}.Values.{plFieldName}.{nameof(IPlaylistReference.Index)}",
                                Math.Min(oldIndex, newIndex),
                                Math.Max(oldIndex, newIndex))))
                        .ForUpdate()
                        .ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = reader.Current.AsDocument;

                            var playlistReference = doc[nameof(IFile.PlaylistReferences)]["Values"][plFieldName];

                            var indexValue = playlistReference["Index"].AsInt32;

                            if (moveUp)
                            {
                                indexValue--;
                            }
                            else
                            {
                                indexValue++;
                            }

                            playlistReference["Index"] = indexValue;

                            yield return doc;
                        }
                    }
                };

                BsonCollection.Update(transformDocs());
            }

            Database.BeginTrans();

            try
            {
                var file = Collection.FindById(fileId);

                if (file == null)
                {
                    return Task.FromResult((IFile)null);
                }

                var playlistRefernces = new List<IPlaylistReference>();
                if (file.PlaylistReferences != null)
                {
                    playlistRefernces.AddRange(file.PlaylistReferences);
                }

                var queryForPlaylist = Query.EQ(idPath, playlist.Id);
                var reference = (LiteDbPlaylistReference)playlistRefernces.FirstOrDefault(it => it.Id == playlist.Id);

                if (index == null)
                {
                    if (reference == null)
                    {
                        return Task.FromResult((IFile)file);
                    }

                    update(reference.Index, int.MaxValue);

                    playlistRefernces.Remove(reference);
                    file.PlaylistReferences = playlistRefernces.ToArray();
                }
                else
                {
                    var count = Collection.Count(queryForPlaylist);

                    if (reference == null)
                    {
                        count++;
                    }

                    if (index < 0 || index > count)
                    {
                        index = count - 1;
                    }

                    if (reference != null)
                    {
                        if (reference.Index == index.Value)
                        {
                            return Task.FromResult((IFile)file);
                        }

                        update(reference.Index, index.Value);

                        reference.Index = index.Value;
                    }
                    else
                    {
                        playlistRefernces.Add(new LiteDbPlaylistReference
                        {
                            AddedOn = DateTime.UtcNow,
                            Id = playlist.Id,
                            Index = index.Value
                        });
                        file.PlaylistReferences = playlistRefernces.ToArray();
                    }
                }

                Collection.Update(file);

                return Task.FromResult((IFile)file);
            }
            catch
            {
                successful = false;
                Database.Rollback();
                throw;
            }
            finally
            {
                if (successful)
                {
                    Database.Commit();
                }
            }
        }

        public Task<IFile> Update(Guid fileId, UpdateFileRequest request, IThumbnail[] thumbnails = null)
        {
            var file = Collection.FindById(fileId);

            if (file == null)
            {
                return Task.FromResult((IFile)null);
            }

            file.Description = request.Description ?? file.Description;
            file.Name = request.Name ?? file.Name;
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
                Location = file.Location,
                Md5 = file.Md5,
                ReadRoles = request.ReadRoles ?? new HashSet<string>(),
                Thumbnails = file.Thumbnails?.Select(it => new LiteDbThumbnail(it)).ToArray(),
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

        [BsonId]
        public Guid Id { get; set; }

        public Guid Md5 { get; set; }

        public Guid UploadedBy { get; set; }

        public HashSet<string> ReadRoles { get; set; }

        public HashSet<string> UpdateRoles { get; set; }

        public IPlaylistReference[] PlaylistReferences { get; set; }

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

    public class LiteDbPlaylistReference : IPlaylistReference
    {
        public LiteDbPlaylistReference()
        {
        }
        public LiteDbPlaylistReference(IPlaylistReference playlistReference)
        {
            AddedOn = playlistReference.AddedOn;
            Id = playlistReference.Id;
            Index = playlistReference.Index;
        }

        public DateTime AddedOn { get; set; }

        public Guid Id { get; set; }

        public int Index { get; set; }
    }
}
