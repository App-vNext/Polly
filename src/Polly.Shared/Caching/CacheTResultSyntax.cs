using Polly.Caching;
using System;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>.
        /// If the <paramref name="cacheProvider"/> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider cacheProvider, TimeSpan ttl)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));

            return Cache<TResult>(cacheProvider.As<TResult>(), new TimeSpanTtl(ttl), DefaultCacheKeyStrategy.Instance);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>.
        /// If the <paramref name="cacheProvider"/> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider cacheProvider, ITtlStrategy ttlStrategy)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));

            return Cache<TResult>(cacheProvider.As<TResult>(), ttlStrategy, DefaultCacheKeyStrategy.Instance);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>.
        /// If the <paramref name="cacheProvider"/> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider<TResult> cacheProvider, TimeSpan ttl)
        {
            return Cache<TResult>(cacheProvider, new TimeSpanTtl(ttl), DefaultCacheKeyStrategy.Instance);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy"/> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider"/> holds a value for the cache key specified by <see cref="M:Context.ExecutionKey"/>.
        /// If the <paramref name="cacheProvider"/> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider"/> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider"/>, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider<TResult> cacheProvider, ITtlStrategy ttlStrategy)
        {
            return Cache<TResult>(cacheProvider, ttlStrategy, DefaultCacheKeyStrategy.Instance);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider cacheProvider, TimeSpan ttl, ICacheKeyStrategy cacheKeyStrategy)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));

            return Cache<TResult>(cacheProvider.As<TResult>(), new TimeSpanTtl(ttl), cacheKeyStrategy);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider cacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));

            return Cache<TResult>(cacheProvider.As<TResult>(), ttlStrategy, cacheKeyStrategy);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider<TResult> cacheProvider, TimeSpan ttl, ICacheKeyStrategy cacheKeyStrategy)
        {
            return Cache<TResult>(cacheProvider, new TimeSpanTtl(ttl), cacheKeyStrategy);
        }

        /// <summary>
        /// <para>Builds a <see cref="Policy" /> that will function like a result cache for delegate executions returning a <typeparamref name="TResult"/>.</para>
        /// <para>Before executing a delegate, checks whether the <paramref name="cacheProvider" /> holds a value for the cache key determined by applying the <paramref name="cacheKeyStrategy"/> to the execution <see cref="Context"/>.
        /// If the <paramref name="cacheProvider" /> contains a value, returns that value and does not execute the governed delegate.  If the <paramref name="cacheProvider" /> does not provide a value, executes the governed delegate, stores the value with the <paramref name="cacheProvider" />, then returns the value.
        /// </para>
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="ttlStrategy">A strategy for specifying ttl for values to be cached.</param>
        /// <param name="cacheKeyStrategy">The cache key strategy.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentNullException">cacheProvider</exception>
        /// <exception cref="ArgumentNullException">ttlStrategy</exception>
        /// <exception cref="ArgumentNullException">cacheKeyStrategy</exception>
        public static CachePolicy<TResult> Cache<TResult>(ICacheProvider<TResult> cacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            if (ttlStrategy == null) throw new ArgumentNullException(nameof(ttlStrategy));
            if (cacheKeyStrategy == null) throw new ArgumentNullException(nameof(cacheKeyStrategy));

            return new CachePolicy<TResult>(cacheProvider, ttlStrategy, cacheKeyStrategy);
        }
    }
}
