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
    public class LiteDbPlaylists : IPlaylists
    {
        public LiteDbPlaylists(ILiteDatabase db)
        {
            Collection = db.GetCollection<LiteDbPlaylist>("playlists");
            Database = db;
        }

        public ILiteCollection<LiteDbPlaylist> Collection { get; }
        public ILiteDatabase Database { get; }

        public Task<IPlaylist> Create(CreatePlaylistRequest request, Guid playlistId, Guid userId, IThumbnail[] thumbnails = null)
        {
            var liteDbPlaylist = new LiteDbPlaylist
            {
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                Description = request.Description ?? "",
                Id = playlistId,
                Name = request.Name ?? "",
                ReadRoles = request.ReadRoles ?? new RoleSet(),
                Thumbnails = thumbnails?.Select(it => new LiteDbThumbnail(it)).ToArray(),
                UpdateRoles = request.UpdateRoles ?? new RoleSet()
            };

            Collection.Insert(liteDbPlaylist);

            return Task.FromResult((IPlaylist)liteDbPlaylist);
        }

        public Task<IPlaylist> Get(Guid playlistId) =>
            Task.FromResult((IPlaylist)Collection.FindById(playlistId));

        public Task<IPlaylist[]> Get(IEnumerable<Guid> playlistIds) =>
            Task.FromResult(Collection.Find(Query.In("_id", new BsonArray(playlistIds.Select(it => new BsonValue(it))))).Cast<IPlaylist>().ToArray());

        public Task<IPlaylist> GetByName(string name) =>
            Task.FromResult((IPlaylist)Collection.FindOne(Query.EQ(nameof(LiteDbPlaylist.Name), name)));

        public Task<SearchPlaylistsResponse<IPlaylist>> Search(SearchPlaylistsRequest request, Guid userId, RoleSet userRoles)
        {
            var query = Query.All();

            query.Offset = request.Skip;
            query.Limit = request.Take;

            if (!userRoles.Contains(RequiresAdminRoleAttribute.AdminRole))
            {
                var accessFilter = new List<BsonExpression>
                {
                    Query.EQ(nameof(LiteDbPlaylist.CreatedBy), userId)
                };

                foreach (var role in userRoles)
                {
                    accessFilter.Add(Query.EQ($"{nameof(LiteDbPlaylist.ReadRoles)}[*] ANY", role));
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

            switch (request.Sort)
            {
                case PlaylistSortOptions.CreatedOn:
                    query.OrderBy = nameof(LiteDbPlaylist.CreatedOn);
                    break;
                case PlaylistSortOptions.Name:
                    query.OrderBy = nameof(LiteDbPlaylist.Name);
                    break;
            }

            query.Order = request.Ascending ? Query.Ascending : Query.Descending;

            var results = Collection.Find(query).ToArray();

            var response = new SearchPlaylistsResponse<IPlaylist>(request, 0, results.Cast<IPlaylist>());

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

        public Task<IPlaylist> Update(Guid playlistId, UpdatePlaylistRequest request, IThumbnail[] thumbnails = null)
        {
            var liteDbPlaylist = Collection.FindById(playlistId);

            if (liteDbPlaylist == null)
            {
                return Task.FromResult((IPlaylist)null);
            }

            liteDbPlaylist.Description = request.Description ?? liteDbPlaylist.Description;
            liteDbPlaylist.Name = request.Name ?? liteDbPlaylist.Name;
            liteDbPlaylist.ReadRoles = request.ReadRoles ?? liteDbPlaylist.ReadRoles;
            liteDbPlaylist.Thumbnails = thumbnails ?? liteDbPlaylist.Thumbnails;
            liteDbPlaylist.UpdateRoles = request.UpdateRoles ?? liteDbPlaylist.UpdateRoles;

            Collection.Update(liteDbPlaylist.Id, liteDbPlaylist);

            return Task.FromResult((IPlaylist)liteDbPlaylist);
        }
    }

    public class LiteDbPlaylist : IPlaylist
    {
        public DateTime CreatedOn { get; set; }

        [BsonId]
        public Guid Id { get; set; }

        public Guid CreatedBy { get; set; }

        public RoleSet ReadRoles { get; set; }

        public RoleSet UpdateRoles { get; set; }

        public IThumbnail[] Thumbnails { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }
    }
}
