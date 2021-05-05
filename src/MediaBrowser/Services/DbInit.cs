using LiteDB;
using MediaBrowser.Attributes;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class DbInit : IHaveInit
    {
        public DbInit(AuthConfig config, IRoles roles, IUsers users)
        {
            Config = config;
            Roles = roles;
            Users = users;
        }

        public AuthConfig Config { get; }
        public IRoles Roles { get; }
        public IUsers Users { get; }

        public async Task Init()
        {
            BsonMapper.Global.RegisterType<IThumbnail>
            (
                serialize: it => BsonMapper.Global.Serialize(new LiteDbThumbnail(it)),
                deserialize: BsonMapper.Global.Deserialize<LiteDbThumbnail>
            );

            BsonMapper.Global.RegisterType
            (
                serialize: playlists =>
                {
                    if (playlists == null || playlists.Length == 0)
                    {
                        return BsonValue.Null;
                    }

                    var ids = new HashSet<Guid>();
                    var values = new BsonDocument();

                    foreach (var playlist in playlists)
                    {
                        if (playlist != null && ids.Add(playlist.Id))
                        {
                            values.Add($"PL_{playlist.Id.ToString().Replace('-', '_')}", BsonMapper.Global.Serialize(new LiteDbPlaylistReference(playlist)));
                        }
                    }

                    if (ids.Count == 0)
                    {
                        return BsonValue.Null;
                    }

                    return new BsonDocument
                    {
                        { "Ids", new BsonArray(ids.Select(it => new BsonValue(it))) },
                        { "Values", values }
                    };
                },
                deserialize: it => it == null || !it.IsDocument || !it.AsDocument.TryGetValue("Values", out var values) || !values.IsDocument ? null :
                    values.AsDocument
                        .Select(it => it.Value)
                        .OfType<BsonDocument>()
                        .Select(BsonMapper.Global.Deserialize<LiteDbPlaylistReference>)
                        .Cast<IPlaylistReference>()
                        .ToArray()
            );

            var role = await Roles.GetByName(RequiresAdminRoleAttribute.AdminRole);
            if (role == null)
            {
                await Roles.Create(new CreateRoleRequest
                {
                    Description = "The admin role for user and role management.",
                    Name = RequiresAdminRoleAttribute.AdminRole
                });
            }

            var response = await Users.Search(new SearchUsersRequest
            {
                Take = 2
            });
            if (response.Results.Length == 0)
            {
                await Users.Create(new CreateUserRequest
                {
                    FirstName = Config.InitFirstName,
                    LastName = Config.InitLastName,
                    Password = Config.InitUserPassword,
                    Roles = new [] { RequiresAdminRoleAttribute.AdminRole },
                    UserName = Config.InitUserName
                });
            }
        }
    }
}
