using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MediaBrowser.Attributes;
using MediaBrowser.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    [Installer(Group = nameof(MediaFileProcessing), Priority = int.MaxValue)]
    public class MediaFileProcessing : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (!container.Kernel.HasComponent(typeof(IFfmpeg)))
            {
                container.Register(Component.For<IHaveInit, IFfmpeg>().ImplementedBy<Ffmpeg>().LifestyleSingleton());
            }

            if (!container.Kernel.HasComponent(typeof(IUploadRequestProcessor)))
            {
                container.Register(Component.For<IUploadRequestProcessor>().ImplementedBy<UploadRequestProcessor>().LifestyleSingleton());
            }
        }
    }
}
