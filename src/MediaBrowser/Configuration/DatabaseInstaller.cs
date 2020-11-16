using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LiteDB;
using MediaBrowser.Attributes;
using MediaBrowser.Services;
using System;
using System.IO;

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
                Component.For<IRoles>().ImplementedBy<LiteDbRoles>().LifestyleSingleton(),
                Component.For<IUsers>().ImplementedBy<LiteDbUsers>().LifestyleSingleton());
        }
    }
}
