using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LiteDB;
using MediaBrowser.Attributes;
using MediaBrowser.Models;
using MediaBrowser.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    [Installer(Group = nameof(DatabaseInstaller), Priority = int.MaxValue)]
    public class DatabaseInstaller : IWindsorInstaller
    {
        public DatabaseInstaller(DatabaseConfiguration config, DiskLocations diskLocations)
        {
            Config = config;
            DiskLocations = diskLocations;
        }

        public DatabaseConfiguration Config { get; }
        public DiskLocations DiskLocations { get; }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IHaveInit>().ImplementedBy<DbInit>().LifestyleSingleton());

            if (!string.IsNullOrEmpty(Config.ConnectionStringType) && !string.Equals(Config.ConnectionStringType, "LiteDb", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

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

            BsonMapper.Global.RegisterType
            (
                serialize: it => new BsonArray(it.Select(role => new BsonValue(role))),
                deserialize: it => it != null && it.IsArray ? new RoleSet(it.AsArray
                    .Where(it => it != null && it.Type == BsonType.String)
                    .Select(it => it.AsString)) : null
            );

            var connectionString = Config.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                var tempDir = DiskLocations.Temp ?? Path.GetTempPath();

                var fileName = Path.Combine(tempDir, "MedaiBrowser.db");

                connectionString = $"Upgrade=true;Filename={fileName}";
            }

            var connection = new LiteDatabase(connectionString);

            container.Register(
                Component.For<ILiteDatabase>().Instance(connection).LifestyleSingleton(),
                Component.For<IFiles>().ImplementedBy<LiteDbFiles>().LifestyleSingleton(),
                Component.For<IPlaylists>().ImplementedBy<LiteDbPlaylists>().LifestyleSingleton(),
                Component.For<IRoles>().ImplementedBy<LiteDbRoles>().LifestyleSingleton(),
                Component.For<IUsers>().ImplementedBy<LiteDbUsers>().LifestyleSingleton());
        }
    }
}
