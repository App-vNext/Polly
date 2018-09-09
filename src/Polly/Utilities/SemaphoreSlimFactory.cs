using SemaphoreSlim = System.Threading.SemaphoreSlim;

namespace Polly.Utilities
{
    /// <summary>
    /// A factory class for the different implementations of SemaphoreSlim used by Polly.
    /// </summary>
    public static class SemaphoreSlimFactory
    {
        /// <summary>
        /// Creates the semaphore slim.
        /// </summary>
        /// <param name="maxParallelization">The maximum parallelization.</param>
        /// <returns>SemaphoreSlim.</returns>
        public static SemaphoreSlim CreateSemaphoreSlim(int maxParallelization) => new SemaphoreSlim(maxParallelization, maxParallelization);
    }
}
