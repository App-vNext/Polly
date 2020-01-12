using System.Threading.Tasks;

namespace Polly.Specs.Helpers
{
    /// <summary>
    /// Task helpers.
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// Defines a completed Task for use as a completed, empty asynchronous delegate.
        /// </summary>
        public static readonly Task EmptyTask = Task.CompletedTask; // This should now be inlined, given all targets support it. To do in its own PR, to avoid creating noise in other PRs.
    }
}
