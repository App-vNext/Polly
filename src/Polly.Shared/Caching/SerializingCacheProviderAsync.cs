using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// Defines an <see cref="IAsyncCacheProvider"/> which serializes objects of any type in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with asynchronous <see cref="CachePolicy" />.
    /// </summary>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    public class SerializingCacheProviderAsync<TSerialized> : IAsyncCacheProvider
    {
        private readonly IAsyncCacheProvider<TSerialized> _wrappedCacheProvider;
        private readonly ICacheItemSerializer<object, TSerialized> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializingCacheProviderAsync{TResult, TSerialized}"/> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">wrappedCacheProvider </exception>
        /// <exception cref="System.ArgumentNullException">serializer </exception>
        public SerializingCacheProviderAsync(IAsyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<object, TSerialized> serializer)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _wrappedCacheProvider = wrappedCacheProvider;
            _serializer = serializer;
        }

        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task{TResult}" /> promising as Result the value from cache; or null, if none was found.</returns>
        public async Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return _serializer.Deserialize(
                await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext));
        }

        /// <summary>
        /// Puts the specified value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
        public async Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await _wrappedCacheProvider.PutAsync(
                           key,
                           _serializer.Serialize(value),
                           ttl,
                           cancellationToken,
                           continueOnCapturedContext
                       ).ConfigureAwait(continueOnCapturedContext);
        }
    }

    /// <summary>
    /// Defines an <see cref="IAsyncCacheProvider{TResult}"/> which serializes objects of type <typeparamref name="TResult"/> in and out of an underlying cache which caches as type <typeparamref name="TSerialized"/>.  For use with asynchronous <see cref="CachePolicy" />.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <typeparam name="TSerialized">The type of serialized objects to be placed in the cache.</typeparam>
    public class SerializingCacheProviderAsync<TResult, TSerialized> : IAsyncCacheProvider<TResult>
    {
        private readonly IAsyncCacheProvider<TSerialized> _wrappedCacheProvider;
        private readonly ICacheItemSerializer<TResult, TSerialized> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializingCacheProviderAsync{TResult, TSerialized}"/> class.
        /// </summary>
        /// <param name="wrappedCacheProvider">The wrapped cache provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">wrappedCacheProvider </exception>
        /// <exception cref="System.ArgumentNullException">serializer </exception>
        public SerializingCacheProviderAsync(IAsyncCacheProvider<TSerialized> wrappedCacheProvider, ICacheItemSerializer<TResult, TSerialized> serializer)
        {
            if (wrappedCacheProvider == null) throw new ArgumentNullException(nameof(wrappedCacheProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _wrappedCacheProvider = wrappedCacheProvider;
            _serializer = serializer;
        }

        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task{TResult}" /> promising as Result the value from cache; or null, if none was found.</returns>
        public async Task<TResult> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return _serializer.Deserialize(
                await _wrappedCacheProvider.GetAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext));
        }

        /// <summary>
        /// Puts the specified value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to put into the cache.</param>
        /// <param name="ttl">The time-to-live for the cache entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether async calls should continue on a captured synchronization context.</param>
        /// <returns>A <see cref="Task" /> which completes when the value has been cached.</returns>
        public async Task PutAsync(string key, TResult value, Ttl ttl, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await _wrappedCacheProvider.PutAsync(
                           key,
                           _serializer.Serialize(value),
                           ttl,
                           cancellationToken,
                           continueOnCapturedContext
                       ).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
