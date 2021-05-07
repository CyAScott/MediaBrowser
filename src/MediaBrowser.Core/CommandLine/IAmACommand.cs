using System.Threading.Tasks;

namespace MediaBrowser.CommandLine
{
    /// <summary>
    /// A command handler.
    /// </summary>
    public interface IAmACommand
    {
        /// <summary>
        /// Invokes the command.
        /// </summary>
        Task Invoke(string[] args);
    }
}
