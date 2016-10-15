using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// Defines methods for classes providing mapping of cached values for asynchronous cache functionality for Polly <see cref="CachePolicy"/>s.
    /// </summary>
    public abstract class MappingCacheProviderAsync : ICacheProviderAsync
    {
        private readonly ICacheProviderAsync _wrappedCacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingCacheProviderAsync"/> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        protected MappingCacheProviderAsync(ICacheProviderAsync wrappedCacheProvider)
        {
            _wrappedCacheProvider = wrappedCacheProvider;
        }

        /// <summary>
        /// Asynchronously maps an object retrieved from the cache back to the native or original format before it was cached. 
        /// </summary>
        /// <param name="rawObjectFromCache">The raw object from cache.</param>
        /// <param name="continueOnCapturedContext">if set to <c>true</c>, the implementation should continue on captured context whenever it awaits.</param>
        /// <returns>The object mapped back to native format.</returns>
        public abstract Task<object> GetMapper(object rawObjectFromCache, bool continueOnCapturedContext);

        /// <summary>
        /// Asynchronously maps a native object to an amended form for caching.
        /// </summary>
        /// <param name="nativeObject">The native object.</param>
        /// <param name="continueOnCapturedContext">if set to <c>true</c>, the implementation should continue on captured context whenever it awaits.</param>
        /// <returns>The object mapped to caching format.</returns>
        public abstract Task<object> PutMapper(object nativeObject, bool continueOnCapturedContext);

        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task{TResult}" /> promising as Result the value from cache; or null, if none was found.</returns>
        public async Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return GetMapper(
                await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext), 
                continueOnCapturedContext
                );
        }

        /// <summary>
        /// Puts the specified value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
        public async Task PutAsync(string key, object value, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _wrappedCacheProvider.PutAsync(
                    key, 
                    await PutMapper(value, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext), 
                    cancellationToken,
                    continueOnCapturedContext
                ).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
