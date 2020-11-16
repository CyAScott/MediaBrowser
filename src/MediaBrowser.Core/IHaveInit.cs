using System.Threading.Tasks;

namespace MediaBrowser
{
    /// <summary>
    /// Adds a method that is called before the project starts.
    /// </summary>
    public interface IHaveInit
    {
        /// <summary>
        /// Called before the project starts.
        /// </summary>
        Task Init();
    }
}
