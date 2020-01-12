using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.Caching
{
    /// <summary>
    /// A cache policy that can be applied to synchronous executions.
    /// </summary>
    public class CachePolicy : Policy, ISyncCachePolicy
    {
        private readonly ISyncCacheProvider _syncCacheProvider;
        private readonly ITtlStrategy _ttlStrategy;
        private readonly Func<Context, string> _cacheKeyStrategy;

        private readonly Action<Context, string> _onCacheGet;
        private readonly Action<Context, string> _onCacheMiss;
        private readonly Action<Context, string> _onCachePut;
        private readonly Action<Context, string, Exception> _onCacheGetError;
        private readonly Action<Context, string, Exception> _onCachePutError;

        internal CachePolicy(
            ISyncCacheProvider syncCacheProvider, 
            ITtlStrategy ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
        {
            _syncCacheProvider = syncCacheProvider;
            _ttlStrategy = ttlStrategy;
            _cacheKeyStrategy = cacheKeyStrategy;

            _onCacheGet = onCacheGet;
            _onCachePut = onCachePut;
            _onCacheMiss = onCacheMiss;
            _onCacheGetError = onCacheGetError;
            _onCachePutError = onCachePutError;
        }

        /// <inheritdoc/>
        protected override void SyncNonGenericImplementation(in ISyncExecutable action, Context context, CancellationToken cancellationToken)
            // Pass-through/NOOP policy action, for void-returning calls through a cache policy.
            => action.Execute(context, cancellationToken);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            return CacheEngine.Implementation<TExecutable, TResult>(
                _syncCacheProvider.For<TResult>(),
                _ttlStrategy.For<TResult>(),
                _cacheKeyStrategy,
                action,
                context,
                cancellationToken,
                _onCacheGet,
                _onCacheMiss,
                _onCachePut,
                _onCacheGetError,
                _onCachePutError);
        }
    }

    /// <summary>
    /// A cache policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class CachePolicy<TResult> : Policy<TResult>, ISyncCachePolicy<TResult>
    {
        private readonly ISyncCacheProvider<TResult> _syncCacheProvider;
        private readonly ITtlStrategy<TResult> _ttlStrategy;
        private readonly Func<Context, string> _cacheKeyStrategy;

        private readonly Action<Context, string> _onCacheGet;
        private readonly Action<Context, string> _onCacheMiss;
        private readonly Action<Context, string> _onCachePut;
        private readonly Action<Context, string, Exception> _onCacheGetError;
        private readonly Action<Context, string, Exception> _onCachePutError;

        internal CachePolicy(
            ISyncCacheProvider<TResult> syncCacheProvider,
            ITtlStrategy<TResult> ttlStrategy,
            Func<Context, string> cacheKeyStrategy,
            Action<Context, string> onCacheGet,
            Action<Context, string> onCacheMiss,
            Action<Context, string> onCachePut,
            Action<Context, string, Exception> onCacheGetError,
            Action<Context, string, Exception> onCachePutError)
        {
            _syncCacheProvider = syncCacheProvider;
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
        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => CacheEngine.Implementation(
                _syncCacheProvider,
                _ttlStrategy,
                _cacheKeyStrategy,
                action,
                context,
                cancellationToken,
                _onCacheGet,
                _onCacheMiss,
                _onCachePut,
                _onCacheGetError,
                _onCachePutError);
    }
}
