﻿using System;
using System.Diagnostics;
using System.Threading;
using Polly.Utilities;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to delegates.
    /// </summary>
    public class FallbackPolicy : Policy, IFallbackPolicy
    {
        private Action<Exception, Context> _onFallback;
        private Action<Exception, Context, CancellationToken> _fallbackAction;

        internal FallbackPolicy(
            PolicyBuilder policyBuilder,
            Action<Exception, Context> onFallback,
            Action<Exception, Context, CancellationToken> fallbackAction)
            : base(policyBuilder)
        {
            _onFallback = onFallback ?? throw new ArgumentNullException(nameof(onFallback));
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
            => FallbackEngine.Implementation<EmptyStruct>(
                (ctx, token) => { action(ctx, token); return EmptyStruct.Instance; }, 
                context, 
                cancellationToken, 
                ExceptionPredicates,
                ResultPredicates<EmptyStruct>.None,
                (outcome, ctx) => _onFallback(outcome.Exception, ctx),
                (outcome, ctx, ct) => { _fallbackAction(outcome.Exception, ctx, ct); return EmptyStruct.Instance; });

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Whatever>.Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
    }

    /// <summary>
    /// A fallback policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public class FallbackPolicy<TResult> : Policy<TResult>, IFallbackPolicy<TResult>
    {
        private Action<DelegateResult<TResult>, Context> _onFallback;
        private Func<DelegateResult<TResult>, Context, CancellationToken, TResult> _fallbackAction;

        internal FallbackPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Action<DelegateResult<TResult>, Context> onFallback,
            Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction
            ) : base(policyBuilder)
        {
            _onFallback = onFallback ?? throw new ArgumentNullException(nameof(onFallback));
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => FallbackEngine.Implementation(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _onFallback,
                _fallbackAction);
    }
}