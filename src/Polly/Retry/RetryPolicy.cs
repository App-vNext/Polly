using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to synchronous executions.
    /// </summary>
    public class RetryPolicy : PolicyV8, ISyncRetryPolicy
    {
        private readonly Action<Exception, TimeSpan, int, Context> _onRetry;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, Exception, Context, TimeSpan> _sleepDurationProvider;

        internal RetryPolicy(
            PolicyBuilder policyBuilder,
            Action<Exception, TimeSpan, int, Context> onRetry, 
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null
            ) 
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
        }

        /// <inheritdoc/>
        protected override TResult SyncGenericImplementationV8<TExecutable, TResult>(in TExecutable action,
            Context context, CancellationToken cancellationToken)
            => RetryEngineV8.Implementation(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<TResult>.None,
                _onRetry == null ? (Action<DelegateResult<TResult>, TimeSpan, int, Context>)null : (outcome, timespan, retryCount, ctx) => _onRetry(outcome.Exception, timespan, retryCount, ctx),
                _permittedRetryCount,
                _sleepDurationsEnumerable,
                _sleepDurationProvider != null
                    ? (retryCount, outcome, ctx) => _sleepDurationProvider(retryCount, outcome.Exception, ctx)
                    : (Func<int, DelegateResult<TResult>, Context, TimeSpan>)null
                );
    }

    /// <summary>
    /// A retry policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class RetryPolicy<TResult> : PolicyV8<TResult>, ISyncRetryPolicy<TResult>
    {
        private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

        internal RetryPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementationV8<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => RetryEngineV8.Implementation(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _onRetry,
                _permittedRetryCount,
                _sleepDurationsEnumerable, 
                _sleepDurationProvider
            );
    }
}