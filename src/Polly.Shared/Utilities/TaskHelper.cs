using System.Threading.Tasks;

namespace Polly.Utilities
{
    /// <summary>
    /// Task helpers.
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// Defines a completed Task for use as a completed, empty asynchronous delegate.
        /// </summary>
        public static Task EmptyTask = Task.FromResult(true);
    }
}
