using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Caching
{
    public partial class CachePolicy 
    {
        private readonly ICacheProviderAsync _asyncCacheProvider;

        internal CachePolicy(ICacheProviderAsync asyncCacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
            : base((func, context, cancellationToken, continueOnCapturedContext) => func(cancellationToken), // Pass-through/NOOP policy action, for void-returning executions through the cache policy.
                PredicateHelper.EmptyExceptionPredicates)
        {
            _asyncCacheProvider = asyncCacheProvider;
            _ttlStrategy = ttlStrategy;
            _cacheKeyStrategy = cacheKeyStrategy;
        }

        /// <summary>
        /// Executes the specified action asynchronously within the cache policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="context">Execution context that is passed to the exception policy; defines the cache key to use in cache lookup.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action, or the cache.</returns>
        public override Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return CacheEngine.ImplementationAsync<TResult>(_asyncCacheProvider.AsyncAs<TResult>(), _ttlStrategy, _cacheKeyStrategy, action, context, cancellationToken, continueOnCapturedContext);
        }
    }

    public partial class CachePolicy<TResult>
    {
        internal CachePolicy(ICacheProviderAsync<TResult> asyncCacheProvider, ITtlStrategy ttlStrategy, ICacheKeyStrategy cacheKeyStrategy)
            : base((func, context, cancellationToken, continueOnCapturedContext) => CacheEngine.ImplementationAsync(asyncCacheProvider, ttlStrategy, cacheKeyStrategy, func, context, cancellationToken, continueOnCapturedContext),
                PredicateHelper.EmptyExceptionPredicates,
                Enumerable.Empty<ResultPredicate<TResult>>())
        { }
    }

}
