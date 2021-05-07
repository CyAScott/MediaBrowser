using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MediaBrowser.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }
        public IServiceCollection Services { get; private set; }
        public StartupConfigureServices StartupConfigureServices { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var hosts = Configuration["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (hosts?.Length > 0)
            {
                services.Configure<HostFilteringOptions>(options => options.AllowedHosts = hosts);
            }

            Services = services;

            Services
                .AddMvc(options => StartupConfigureServices.Config(options))
                .AddNewtonsoftJson(options => StartupConfigureServices.Config(options));

            Services.AddSwaggerGenNewtonsoftSupport();

            Services.AddSwaggerGen(config => StartupConfigureServices.Config(config));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            var container = serviceProvider.GetService<IWindsorContainer>();

            container.Register(
                Component.For<IApplicationBuilder>().Instance(app).LifestyleSingleton(),
                Component.For<IWebHostEnvironment>().Instance(env).LifestyleSingleton());

            if (!container.Kernel.HasComponent(typeof(IStartupConfigure)))
            {
                container.Register(Component.For<IStartupConfigure>().ImplementedBy<StartupConfigure>().LifestyleSingleton());
            }

            var service = container.Resolve<IStartupConfigure>();

            service.Configure().Wait();
        }
    }
}
