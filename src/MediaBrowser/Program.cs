using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using MediaBrowser.Attributes;
using MediaBrowser.CommandLine;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Console;

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

        private static void printHelp(Dictionary<string, Type> commands)
        {
            WriteLine(new string('_', 80));
            WriteLine();
            WriteLine("Media Browser Help");
            WriteLine(new string('_', 80));
            WriteLine();

            foreach (var command in commands.OrderBy(it => it.Key))
            {
                var name = command.Key;
                var service = command.Value;

                var description = service.GetCustomAttribute<CommandInfoAttribute>() ?? new CommandInfoAttribute("");

                description.PrintInfo(service, name);
            }
        }

        public static async Task<int> Main(string[] args)
        {
            var commands = Assembly.Load("MediaBrowser.Core")
                .ExportedTypes
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    typeof(IAmACommand).IsAssignableFrom(type) &&
                    Regex.IsMatch(type.Namespace ?? "", @"^MediaBrowser\.CommandLine($|\.)", RegexOptions.IgnoreCase))
                .ToDictionary(type => type.Name, StringComparer.OrdinalIgnoreCase);

            var command = args.FirstOrDefault();
            if (command != null)
            {
                if (string.Equals(command, "?") || string.Equals(command, "help", StringComparison.OrdinalIgnoreCase))
                {
                    printHelp(commands);
                    return 0;
                }

                if (!commands.TryGetValue(command, out var commandServiceType))
                {
                    WriteLine("Command not found!");
                    WriteLine();

                    printHelp(commands);

                    return 1;
                }

                try
                {
                    var container = new WindsorContainer();

                    var builder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.IsNullOrEmpty(environment))
                    {
                        builder = builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
                    }

                    builder = builder.AddEnvironmentVariables();

                    container.Register(
                        Component.For<IConfiguration, IConfigurationRoot>().Instance(builder.Build()).LifestyleSingleton(),
                        Component.For(commandServiceType).ImplementedBy(commandServiceType).LifestyleSingleton());

                    ConfigureContainer(null, container);

                    var init = container.ResolveAll<IHaveInit>();
                    foreach (var serviceInfo in init
                        .Select(service => new
                        {
                            attribute = service.GetType().GetCustomAttribute<InitAttribute>() ?? new InitAttribute(),
                            service
                        })
                        .OrderBy(serviceInfo => serviceInfo.attribute.Priority))
                    {
                        await serviceInfo.service.Init();
                    }

                    var commandService = (IAmACommand)container.Resolve(commandServiceType);

                    if (commandService is CommandLineArgs commandLineArgs)
                    {
                        commandLineArgs.Parse(args);
                    }

                    await commandService.Invoke(args);
                }
                catch (Exception error)
                {
                    WriteLine(error.Message);
                    return 2;
                }
            }
            else
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }

            return 0;
        }

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
            var startup = hostContext?.Properties.Values.OfType<Startup>().Single();
            var serviceCollection = startup?.Services;

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
                Component.For<StartupConfigureServices>().ImplementedBy<StartupConfigureServices>().LifestyleSingleton());

            if (serviceCollection != null)
            {
                container.Register(Component.For<IServiceCollection>().Instance(serviceCollection).LifestyleSingleton());
            }

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

                if (startup != null)
                {
                    startup.StartupConfigureServices = container.Resolve<StartupConfigureServices>();
                }
            }
        }
    }
}
