using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class StartupConfigure : IStartupConfigure
    {
        public StartupConfigure(IApplicationBuilder app, IWebHostEnvironment environment, IWindsorContainer container, VersionInfo versionInfo)
        {
            App = app;
            Container = container;
            Environment = environment;
            VersionInfo = versionInfo;
        }

        public IApplicationBuilder App { get; }
        public IWebHostEnvironment Environment { get; }
        public IWindsorContainer Container { get; }
        public VersionInfo VersionInfo { get; }

        public virtual async Task Configure()
        {
            if (Environment.IsDevelopment())
            {
                App.UseDeveloperExceptionPage();
            }

            foreach (var service in Container.ResolveAll<IHaveInit>())
            {
                await service.Init();
            }

            App.UseRouting();

            App.UseSwagger();

            App.UseSwaggerUI(it => it.SwaggerEndpoint($"/swagger/v{VersionInfo.GetVersion()}/swagger.json", $"Media Browser Api V{VersionInfo.GetVersion()}"));

            App.UseStaticFiles();

            App.UseMvc();
        }
    }
}
