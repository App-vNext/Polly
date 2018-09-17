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
        public static Task EmptyTask = FromResult(true);

        /// <summary>
        /// Creates an already completed <see cref="T:System.Threading.Tasks.Task`1"/> from the specified result.
        /// </summary>
        /// <param name="result">The result from which to create the completed task.</param>
        /// <returns>
        /// The completed task.
        /// </returns>
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
#if NET40
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }

    }
}
