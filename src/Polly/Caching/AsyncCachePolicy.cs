using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// A cache policy that can be applied to asynchronous executions.
    /// </summary>
    public class AsyncCachePolicy : AsyncPolicyV8, IAsyncCachePolicy
    {
        private readonly IAsyncCacheProvider _asyncCacheProvider;
        private readonly ITtlStrategy _ttlStrategy;
        private readonly Func<Context, string> _cacheKeyStrategy;

        private readonly Action<Context, string> _onCacheGet;
        private readonly Action<Context, string> _onCacheMiss;
        private readonly Action<Context, string> _onCachePut;
        private readonly Action<Context, string, Exception> _onCacheGetError;
        private readonly Action<Context, string, Exception> _onCachePutError;

        internal AsyncCachePolicy(
            IAsyncCacheProvider asyncCacheProvider, 
            ITtlStrategy ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
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

        /// <inheritdoc/>
        protected override Task AsyncNonGenericImplementationV8(in IAsyncExecutable action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // Pass-through/NOOP policy action, for void-returning executions through the cache policy.
            return action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncCacheEngineV8.ImplementationAsync<TExecutableAsync, TResult>(
                _asyncCacheProvider.AsyncFor<TResult>(),
                _ttlStrategy.For<TResult>(),
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

    /// <summary>
    /// A cache policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncCachePolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncCachePolicy<TResult>
    {
        private readonly IAsyncCacheProvider<TResult> _asyncCacheProvider;
        private readonly ITtlStrategy<TResult> _ttlStrategy;
        private readonly Func<Context, string> _cacheKeyStrategy;

        private readonly Action<Context, string> _onCacheGet;
        private readonly Action<Context, string> _onCacheMiss;
        private readonly Action<Context, string> _onCachePut;
        private readonly Action<Context, string, Exception> _onCacheGetError;
        private readonly Action<Context, string, Exception> _onCachePutError;

        internal AsyncCachePolicy(
            IAsyncCacheProvider<TResult> asyncCacheProvider,
            ITtlStrategy<TResult> ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
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

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncCacheEngineV8.ImplementationAsync<TExecutableAsync, TResult>(
                _asyncCacheProvider,
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

}
