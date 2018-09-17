#if NET40

using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Polly.Utilities
{
    /// <summary>
    /// Contains extension methods for Nito.AsyncEx.AsyncSemaphore
    /// </summary>
    public static class AsyncSemaphoreExtensions
    {
        /// <summary>
        /// Synchronously waits for a slot in the semaphore to be available. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        /// <param name="timeSpan">A TimeSpan for which to wait.  <remarks>In this implementation, only TimeSpan.Zero is permitted, indicating that an attempt should be made to obtain the semaphore immediately, without further waiting. The implementation is intentionally limited to only what is needed to support Polly's use of the corresponding overload in System.Threading.Tasks.SemaphoreSlim version later than .NET 4.0.</remarks>.</param>
        /// <param name="asyncSemaphore">The <see cref="T:Nito.AsyncEx.AsyncSemaphore"/> instance.</param>
        /// <returns>A bool indicating whether the slot was obtained.</returns>
        public static bool Wait(this AsyncSemaphore asyncSemaphore, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            if (timeSpan != TimeSpan.Zero) { throw new NotSupportedException("This extension method to Nito.AsyncEx.AsyncSemaphore intends only to imitate the behaviour of System.Threading.SemaphoreSlim.Wait(TimeSpan..., ) when the TimeSpan supplied is TimeSpan.Zero."); }

            return !asyncSemaphore.WaitAsync(CancellationTokenHelpers.Canceled).IsCanceled;
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore to be available. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        /// <param name="timeSpan">A TimeSpan for which to wait.  <remarks>In this implementation, only TimeSpan.Zero is permitted, indicating that an attempt should be made to obtain the semaphore immediately, without further waiting. The implementation is intentionally limited to only what is needed to support Polly's use of the corresponding overload in System.Threading.Tasks.SemaphoreSlim version later than .NET 4.0.</remarks>.</param>
        /// <param name="asyncSemaphore">The <see cref="T:Nito.AsyncEx.AsyncSemaphore"/> instance.</param>
        /// <returns>A bool indicating whether the slot was obtained.</returns>
        public static Task<bool> WaitAsync(this AsyncSemaphore asyncSemaphore, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            if (timeSpan != TimeSpan.Zero) { throw new NotSupportedException("This extension method to Nito.AsyncEx.AsyncSemaphore intends only to imitate the behaviour of System.Threading.SemaphoreSlim.WaitAsync(TimeSpan..., ) when the TimeSpan supplied is TimeSpan.Zero."); }

            return TaskEx.FromResult(!asyncSemaphore.WaitAsync(CancellationTokenHelpers.Canceled).IsCanceled);
        }

        /// <summary>
        /// Disposes of managed resources.
        /// </summary>
        /// <param name="asyncSemaphore">The <see cref="T:Nito.AsyncEx.AsyncSemaphore"/> instance.</param>
        public static void Dispose(this AsyncSemaphore asyncSemaphore)
        {
            // None to do.  The original implementation of Nito.AsyncEx.AsyncSemaphore is not IDisposable.
        }
    }
}


#endif