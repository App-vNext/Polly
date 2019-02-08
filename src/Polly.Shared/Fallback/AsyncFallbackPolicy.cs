﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to asynchronous delegates.
    /// </summary>
    public class AsyncFallbackPolicy : AsyncPolicy, IFallbackPolicy
    {
        private Func<Exception, Context, Task> _onFallbackAsync;
        private Func<Exception, Context, CancellationToken, Task> _fallbackAction;

        internal AsyncFallbackPolicy(PolicyBuilder policyBuilder, Func<Exception, Context, Task> onFallbackAsync,
            Func<Exception, Context, CancellationToken, Task> fallbackAction)
           : base(policyBuilder)
        {
            _onFallbackAsync = onFallbackAsync ?? throw new ArgumentNullException(nameof(onFallbackAsync));
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        protected override Task ImplementationAsync(
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncFallbackEngine.ImplementationAsync<EmptyStruct>(
                async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<EmptyStruct>.None,
                (outcome, ctx) => _onFallbackAsync(outcome.Exception, ctx),
                async (outcome, ctx, ct) =>
                {
                    await _fallbackAction(outcome.Exception, ctx, ct).ConfigureAwait(continueOnCapturedContext);
                    return EmptyStruct.Instance;
                },
                continueOnCapturedContext);
        }

        /// <inheritdoc/>
        protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => 
            throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Whatever>.Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
    }

    /// <summary>
    /// A fallback policy that can be applied to delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncFallbackPolicy<TResult> : AsyncPolicy<TResult>, IFallbackPolicy<TResult>
    {
        private Func<DelegateResult<TResult>, Context, Task> _onFallbackAsync;
        private Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> _fallbackAction;

        internal AsyncFallbackPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Func<DelegateResult<TResult>, Context, Task> onFallbackAsync, 
            Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction
            ) : base(policyBuilder)
        {
            _onFallbackAsync = onFallbackAsync ?? throw new ArgumentNullException(nameof(onFallbackAsync));
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncFallbackEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _onFallbackAsync,
                _fallbackAction,
                continueOnCapturedContext);
    }
}