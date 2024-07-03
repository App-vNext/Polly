#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines methods for classes providing asynchronous cache functionality for Polly <see cref="CachePolicy" />s.
/// </summary>
public interface IAsyncCacheProvider
{
#pragma warning disable SA1414
    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context. <para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> promising as Result a tuple whose first element is a value indicating whether
    /// the key was found in the cache, and whose second element is the value from the cache (null if not found).
    /// </returns>
    Task<(bool, object?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore  SA1414

    /// <summary>
    /// Puts the specified value in the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.<para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
    /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
    Task PutAsync(string key, object? value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext);
}

/// <summary>
/// Defines methods for classes providing asynchronous cache functionality for Polly <see cref="CachePolicy{TResult}"/>s.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IAsyncCacheProvider<TResult>
{
#pragma warning disable SA1414
    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context. <para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> promising as Result a tuple whose first element is a value indicating whether
    /// the key was found in the cache, and whose second element is the value from the cache (default(TResult) if not found).
    /// </returns>
    Task<(bool, TResult?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore SA1414

    /// <summary>
    /// Puts the specified value in the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to put into the cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.<para><remarks>Note: if the underlying cache's async API does not support controlling whether to continue on a captured context, async Policy executions with continueOnCapturedContext == true cannot be guaranteed to remain on the captured context.</remarks></para></param>
    /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
    Task PutAsync(string key, TResult? value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext);
}
