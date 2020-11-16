using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class StartupConfigureServices
    {
        public static readonly Type[] FilterInterfaces =
        {
            typeof(IActionFilter),
            typeof(IAsyncActionFilter),
            typeof(IAsyncAuthorizationFilter),
            typeof(IAsyncExceptionFilter),
            typeof(IAsyncResourceFilter),
            typeof(IAsyncResultFilter),
            typeof(IAuthorizationFilter),
            typeof(IExceptionFilter),
            typeof(IResourceFilter),
            typeof(IResultFilter)
        };

        public StartupConfigureServices(
            Assembly[] assemblies,
            IWindsorContainer container,
            VersionInfo versionInfo)
        {
            Assemblies = assemblies;
            Container = container;
            VersionInfo = versionInfo;
        }

        public Assembly[] Assemblies { get; }
        public IWindsorContainer Container { get; }
        public VersionInfo VersionInfo { get; }

        public void Config(MvcNewtonsoftJsonOptions options)
        {
            Container.Register(Component.For<MvcNewtonsoftJsonOptions>().Instance(options).LifestyleSingleton());

            options.SerializerSettings.Converters.Add(new StringEnumConverter());

            foreach (var jsonConverter in Assemblies
                .SelectMany(it => it.ExportedTypes)
                .Where(it => it.IsClass && !it.IsAbstract)
                .Where(it => typeof(JsonConverter).IsAssignableFrom(it)))
            {
                Container.Register(Component.For(typeof(JsonConverter)).ImplementedBy(jsonConverter).LifestyleSingleton());
            }

            foreach (var jsonConverter in Container.ResolveAll<JsonConverter>())
            {
                options.SerializerSettings.Converters.Add(jsonConverter);
            }
        }

        public void Config(MvcOptions options)
        {
            options.EnableEndpointRouting = false;

            foreach (var filterService in Assemblies
                .SelectMany(it => it.ExportedTypes)
                .Where(it => it.IsClass && !it.IsAbstract)
                .Where(it => FilterInterfaces.Any(filterInterface => filterInterface.IsAssignableFrom(it))))
            {
                options.Filters.Add(filterService);
            }
        }

        public void Config(SwaggerGenOptions config)
        {
            config.SwaggerDoc($"v{VersionInfo.GetVersion()}", new OpenApiInfo
            {
                Version = $"v{VersionInfo.GetVersion()}",
                Title = "Media Browser Api"
            });

            foreach (var file in Assemblies
                .GroupBy(assembly => assembly.Location)
                .Select(group => Path.Combine(Path.GetDirectoryName(group.Key) ?? "", Path.GetFileNameWithoutExtension(group.Key) + ".xml"))
                .Where(File.Exists))
            {
                config.IncludeXmlComments(file);
            }

            config.IgnoreObsoleteActions();
        }
    }
}
