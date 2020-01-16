using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to synchronous executions.
    /// </summary>
    public class FallbackPolicy : Policy, ISyncFallbackPolicy
    {
        private readonly Action<Exception, Context> _onFallback;
        private readonly Action<Exception, Context, CancellationToken> _fallbackAction;

        internal FallbackPolicy(
            PolicyBuilder policyBuilder,
            Action<Exception, Context> onFallback,
            Action<Exception, Context, CancellationToken> fallbackAction)
            : base(policyBuilder)
        {
            _onFallback = onFallback;
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override void SyncNonGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => FallbackEngine.Implementation<TExecutable, object>(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<object>.None,
                onFallback: _onFallback == null ? (Action<DelegateResult<object>, Context>)null : (outcome, ctx) => _onFallback(outcome.Exception, ctx),
                fallbackAction: (outcome, ctx, ct) => { _fallbackAction(outcome.Exception, ctx, ct); return null; });

        /// <inheritdoc/>
        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Exception>().Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
    }

    /// <summary>
    /// A fallback policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class FallbackPolicy<TResult> : Policy<TResult>, ISyncFallbackPolicy<TResult>
    {
        private readonly Action<DelegateResult<TResult>, Context> _onFallback;
        private readonly Func<DelegateResult<TResult>, Context, CancellationToken, TResult> _fallbackAction;

        internal FallbackPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Action<DelegateResult<TResult>, Context> onFallback,
            Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallbackAction
            ) : base(policyBuilder)
        {
            _onFallback = onFallback;
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
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