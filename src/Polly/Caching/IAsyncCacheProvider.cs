using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// Defines methods for classes providing asynchronous cache functionality for Polly <see cref="CachePolicy" />s.
    /// </summary>
    public interface IAsyncCacheProvider
    {
        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context. <para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
        /// <returns>A <see cref="Task{TResult}" /> promising as Result the value from cache; or null, if none was found.</returns>
        Task<object> GetAsync(String key, CancellationToken cancellationToken, bool continueOnCapturedContext);

        /// <summary>
        /// Puts the specified value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.<para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
        /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
        Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext);
    }

    /// <summary>
    /// Defines methods for classes providing asynchronous cache functionality for Polly <see cref="CachePolicy{TResult}"/>s.
    /// </summary>
    public interface IAsyncCacheProvider<TResult>
    {
        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context. <para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
        /// <returns>A <see cref="Task{TResult}" /> promising as Result the value from cache; or null, if none was found.</returns>
        Task<TResult> GetAsync(String key, CancellationToken cancellationToken, bool continueOnCapturedContext);

        /// <summary>
        /// Puts the specified value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.<para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
        /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
        Task PutAsync(string key, TResult value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext);
    }
}