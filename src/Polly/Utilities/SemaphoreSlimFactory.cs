#if NET40
using SemaphoreSlim = Nito.AsyncEx.AsyncSemaphore;
#else
using SemaphoreSlim = System.Threading.SemaphoreSlim;
#endif

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
        public static SemaphoreSlim CreateSemaphoreSlim(int maxParallelization)
        {
#if NET40
            return new SemaphoreSlim(maxParallelization);
#else
            return new SemaphoreSlim(maxParallelization, maxParallelization);
#endif
        }
    }
}
