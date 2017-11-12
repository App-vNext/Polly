using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Caching
{
    public partial class CachePolicy 
    {
        private readonly IAsyncCacheProvider _asyncCacheProvider;

        internal CachePolicy(
            IAsyncCacheProvider asyncCacheProvider, 
            ITtlStrategy 
            ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
            : base((func, context, cancellationToken, continueOnCapturedContext) => func(context, cancellationToken), // Pass-through/NOOP policy action, for void-returning executions through the cache policy.
                PredicateHelper.EmptyExceptionPredicates)
        {
            _asyncCacheProvider = asyncCacheProvider;
            _ttlStrategy = ttlStrategy;
            _cacheKeyStrategy = cacheKeyStrategy;

            _onCacheGet = onCacheGet;
            _onCachePut = onCachePut;
            _onCacheMiss = onCacheMiss;
            _onCacheGetError = onCacheGetError;
            _onCachePutError = onCachePutError;
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
        public override Task<TResult> ExecuteAsyncInternal<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return CacheEngine.ImplementationAsync<TResult>(
                _asyncCacheProvider.AsyncFor<TResult>(), 
                _ttlStrategy, 
                _cacheKeyStrategy, 
                action, 
                context, 
                cancellationToken,
                continueOnCapturedContext, 
                _onCacheGet, 
                _onCacheMiss, 
                _onCachePut, 
                _onCacheGetError,
                _onCachePutError);
        }
    }

    public partial class CachePolicy<TResult>
    {
        internal CachePolicy(
            IAsyncCacheProvider<TResult> asyncCacheProvider, 
            ITtlStrategy ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
            : base((func, context, cancellationToken, continueOnCapturedContext) => 
                CacheEngine.ImplementationAsync(
                    asyncCacheProvider, 
                    ttlStrategy, 
                    cacheKeyStrategy, 
                    func, 
                    context, 
                    cancellationToken,
                    continueOnCapturedContext, 
                    onCacheGet, 
                    onCacheMiss, 
                    onCachePut, 
                    onCacheGetError, 
                    onCachePutError),
                PredicateHelper.EmptyExceptionPredicates,
                Enumerable.Empty<ResultPredicate<TResult>>())
        { }
    }

}
