using System.Threading.Tasks;

namespace MediaBrowser.Controller.Plugins
{
    /// <summary>
    /// Interface IServerEntryPoint
    /// </summary>
    public interface IServerEntryPoint
    {
        /// <summary>
        /// Runs this instance.
        /// </summary>
        Task RunAsync();
    }
}
