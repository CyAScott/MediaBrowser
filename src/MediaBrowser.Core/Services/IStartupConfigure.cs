using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// Configure method to create the app's request processing pipeline.
    /// </summary>
    public interface IStartupConfigure
    {
        /// <summary>
        /// Configure method to create the app's request processing pipeline.
        /// </summary>
        Task Configure();
    }
}
