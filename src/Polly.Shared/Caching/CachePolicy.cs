using System;
using System.Linq;
using System.Threading;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// A cache policy that can be applied to the results of delegate executions.
    /// </summary>
    public partial class CachePolicy : Policy
    {
        private readonly ICacheProvider _syncCacheProvider;
        private readonly ITtlStrategy _ttlStrategy;
        private readonly ICacheKeyStrategy _cacheKeyStrategy;

        internal CachePolicy(ICacheProvider syncCacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
            : base((action, context, cancellationToken) => action(cancellationToken), // Pass-through/NOOP policy action, for void-returning calls through a cache policy.
                PredicateHelper.EmptyExceptionPredicates)
        {
            _syncCacheProvider = syncCacheProvider;
            _ttlStrategy = ttlStrategy;
            _cacheKeyStrategy = cacheKeyStrategy;
        }

        /// <summary>
        /// Executes the specified action within the cache policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Execution context that is passed to the exception policy; defines the cache key to use in cache lookup.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value returned by the action, or the cache.</returns>
        public override TResult Execute<TResult>(Func<CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return CacheEngine.Implementation<TResult>(_syncCacheProvider.As<TResult>(), _ttlStrategy, _cacheKeyStrategy, action, context, cancellationToken);
        }
    }

    /// <summary>
    /// A cache policy that can be applied to the results of delegate executions.
    /// </summary>
    public partial class CachePolicy<TResult> : Policy<TResult>
    {
        internal CachePolicy(ICacheProvider<TResult> syncCacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
            : base((action, context, cancellationToken) => CacheEngine.Implementation(syncCacheProvider, ttlStrategy, cacheKeyStrategy, action, context, cancellationToken),
                PredicateHelper.EmptyExceptionPredicates,
                Enumerable.Empty<ResultPredicate<TResult>>())
        { }

    }
}
