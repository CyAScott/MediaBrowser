using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MediaBrowser.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    [Installer(Group = nameof(ConfigurationInstaller), Priority = int.MaxValue)]
    public class ConfigurationInstaller : IWindsorInstaller
    {
        public ConfigurationInstaller(Assembly[] assemblies, IConfiguration configuration)
        {
            Assemblies = assemblies;
            Configuration = configuration;
        }

        public Assembly[] Assemblies { get; }
        public IConfiguration Configuration { get; }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (var configClass in Assemblies
                .SelectMany(it => it.ExportedTypes)
                .Where(it => it.IsClass && !it.IsAbstract)
                .Select(it => new
                {
                    name = it.GetCustomAttribute<ConfigurationAttribute>()?.Name,
                    type = it
                })
                .Where(it => !string.IsNullOrEmpty(it.name) && it.type.GetConstructor(Type.EmptyTypes) != null))
            {
                var configSettings = Activator.CreateInstance(configClass.type);

                Configuration.GetSection(configClass.name).Bind(configSettings);

                container.Register(Component.For(configClass.type.GetInterfaces().Append(configClass.type).ToArray()).Instance(configSettings).LifestyleSingleton());
            }
        }
    }
}
