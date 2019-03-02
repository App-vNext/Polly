﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Caching
{
    /// <summary>
    /// A cache policy that can be applied to the results of delegate executions.
    /// </summary>
    public class AsyncCachePolicy : AsyncPolicy 
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
        protected override Task ImplementationAsync(
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // Pass-through/NOOP policy action, for void-returning executions through the cache policy.
            return action(context, cancellationToken);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncCacheEngine.ImplementationAsync<TResult>(
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
    /// A cache policy that can be applied to the results of delegate executions.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncCachePolicy<TResult> : AsyncPolicy<TResult>
    {
        private IAsyncCacheProvider<TResult> _asyncCacheProvider;
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
        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncCacheEngine.ImplementationAsync<TResult>(
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
