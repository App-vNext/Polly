using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to asynchronous executions.
    /// </summary>
    public class AsyncFallbackPolicy : AsyncPolicyV8, IAsyncFallbackPolicy
    {
        private readonly Func<Exception, Context, Task> _onFallbackAsync;
        private readonly Func<Exception, Context, CancellationToken, Task> _fallbackAction;

        internal AsyncFallbackPolicy(PolicyBuilder policyBuilder, Func<Exception, Context, Task> onFallbackAsync,
            Func<Exception, Context, CancellationToken, Task> fallbackAction)
           : base(policyBuilder)
        {
            _onFallbackAsync = onFallbackAsync;
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        protected override Task AsyncNonGenericImplementationV8(in IAsyncExecutable action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncFallbackEngineV8.ImplementationAsync<IAsyncExecutable<object>, object>(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext, 
                ExceptionPredicates,
                ResultPredicates<object>.None,
                onFallbackAsync: _onFallbackAsync == null ? (Func<DelegateResult<object>, Context, Task>)null : (outcome, ctx) => _onFallbackAsync(outcome.Exception, ctx), 
                fallbackAction: async (outcome, ctx, ct) =>
                {
                    await _fallbackAction(outcome.Exception, ctx, ct).ConfigureAwait(continueOnCapturedContext);
                    return null;
                });
        }

        /// <inheritdoc/>
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
            => throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Exception>().Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
    }

    /// <summary>
    /// A fallback policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncFallbackPolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncFallbackPolicy<TResult>
    {
        private readonly Func<DelegateResult<TResult>, Context, Task> _onFallbackAsync;
        private readonly Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> _fallbackAction;

        internal AsyncFallbackPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Func<DelegateResult<TResult>, Context, Task> onFallbackAsync, 
            Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallbackAction
            ) : base(policyBuilder)
        {
            _onFallbackAsync = onFallbackAsync;
            _fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
            => AsyncFallbackEngineV8.ImplementationAsync(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                ExceptionPredicates,
                ResultPredicates,
                _onFallbackAsync,
                _fallbackAction);
    }
}