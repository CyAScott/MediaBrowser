using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MediaBrowser.Attributes;
using NLog;
using NLog.Config;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Xml;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    [Installer(Group = nameof(NLogInstaller), Priority = int.MaxValue)]
    public class NLogInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (LogManager.Configuration == null)
            {
                using (var file = typeof(NLogInstaller).Assembly.GetManifestResourceStream("MediaBrowser.nlog.config"))
                using (var reader = XmlReader.Create(file))
                {
                    LogManager.Configuration = new XmlLoggingConfiguration(reader, Directory.GetCurrentDirectory());
                }
            }

            if (!container.Kernel.HasComponent(typeof(ILogger)))
            {
                Expression<Func<IKernel, CreationContext, ILogger>> factory = (kernel, context) => LogManager.GetLogger(context.Handler.ComponentModel.Implementation.Name);

                container.Register(Component
                    .For<ILogger>()
                    .UsingFactoryMethod(factory.Compile())
                    .LifestyleTransient());
            }
        }
    }
}
