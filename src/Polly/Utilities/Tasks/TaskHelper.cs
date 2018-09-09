#if NETSTANDARD1_1
using System.Threading.Tasks;

namespace Polly.Utilities.Tasks
{
    /// <summary>
    /// Task helpers.
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// Defines a completed Task for use as a completed, empty asynchronous delegate.
        /// </summary>
        public static Task CompletedTask { get; }= Task.FromResult(true);
    }
}
#endif
