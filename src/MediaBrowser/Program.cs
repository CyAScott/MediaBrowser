using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using MediaBrowser.Attributes;
using MediaBrowser.Configuration;
using MediaBrowser.Filters;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(ctx => new WindsorServiceProviderFactory())
                .ConfigureContainer<IWindsorContainer>(ConfigureContainer)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static void ConfigureContainer(HostBuilderContext hostContext, IWindsorContainer container)
        {
            var configuration = container.Resolve<IConfiguration>();

            var assemblyNames = new HashSet<string>(((string)configuration.GetValue(typeof(string), "DiAssemblyNames"))?.Split('|')?.Where(it => !string.IsNullOrEmpty(it)) ?? new string[0], StringComparer.OrdinalIgnoreCase)
            {
                "MediaBrowser",
                "MediaBrowser.Core",
                "MediaBrowser.Tests"
            };

            var assemblies = assemblyNames.ToArray()
                .Select(name =>
                {
                    try
                    {
                        return Assembly.Load(name);
                    }
                    catch
                    {
                        assemblyNames.Remove(name);
                        return null;
                    }
                })
                .Where(assembly => assembly != null)
                .ToArray();
            var completedInstallers = new HashSet<Guid>();
            var startup = hostContext.Properties.Values.OfType<Startup>().Single();
            var serviceCollection = startup.Services;

            var installers = assemblies
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(type => type.IsClass && typeof(IWindsorInstaller).IsAssignableFrom(type))
                .Select(type => new
                {
                    attribute = type.GetCustomAttribute<InstallerAttribute>() ?? new InstallerAttribute(),
                    ctor = type.GetConstructors().First(),
                    type
                })
                .Select(typeInfo => new
                {
                    typeInfo.ctor,
                    groupName = typeInfo.attribute.Group ?? "",
                    priority = typeInfo.attribute.Priority,
                    typeInfo.type,
                    ctorParams = typeInfo.ctor.GetParameters().ToList()
                })
                .ToDictionary(_ => Guid.NewGuid(), installer => installer);

            container.Register(
                Component.For<Assembly[]>().Instance(assemblies).LifestyleSingleton(),
                Component.For<Jwt>().ImplementedBy<Jwt>().LifestyleSingleton(),
                Component.For<IServiceCollection>().Instance(serviceCollection).LifestyleSingleton(),
                Component.For<StartupConfigureServices>().ImplementedBy<StartupConfigureServices>().LifestyleSingleton());

            while (installers.Count != completedInstallers.Count)
            {
                var ranInstallers = false;

                foreach (var group in installers.Where(it => !completedInstallers.Contains(it.Key)).GroupBy(it => it.Value.groupName))
                {
                    foreach (var (key, installer) in group.OrderBy(it => it.Value.priority).Select(it => (it.Key, it.Value)))
                    {
                        if (installer.ctorParams.Count > 0)
                        {
                            foreach (var ctorParam in installer.ctorParams.ToArray())
                            {
                                if (container.Kernel.HasComponent(ctorParam.ParameterType))
                                {
                                    installer.ctorParams.Remove(ctorParam);
                                }
                            }
                        }

                        if (installer.ctorParams.Count == 0)
                        {
                            ranInstallers = true;

                            container.Install((IWindsorInstaller)installer.ctor.Invoke(installer.ctor
                                .GetParameters()
                                .Select(ctorParam => container.Resolve(ctorParam.ParameterType))
                                .ToArray()));

                            completedInstallers.Add(key);
                        }
                    }
                }

                if (!ranInstallers)
                {
                    throw new InvalidOperationException("Failed to setup the DI container.");
                }

                startup.StartupConfigureServices = container.Resolve<StartupConfigureServices>();
            }
        }
    }
}
