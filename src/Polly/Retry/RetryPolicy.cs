using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Polly.Retry.Settings;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to synchronous delegates.
    /// </summary>
    public class RetryPolicy : Policy, IRetryPolicy
    {
        private readonly IOnRetryCallback _onRetry;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, Exception, Context, TimeSpan> _sleepDurationProvider;

        internal RetryPolicy(
            PolicyBuilder policyBuilder,
            IOnRetryCallback onRetry, 
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null
            ) 
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => RetryEngine.Implementation(
                    action, 
                    context, 
                    cancellationToken,
                    ExceptionPredicates,
                    ResultPredicates<TResult>.None, 
                    new OnRetryCallbackAdapter<TResult>(_onRetry),
                    _permittedRetryCount,
                    _sleepDurationsEnumerable,
                    _sleepDurationProvider != null
                        ? (retryCount, outcome, ctx) => _sleepDurationProvider(retryCount, outcome.Exception, ctx)
                        : (Func<int, DelegateResult<TResult>, Context, TimeSpan>)null
                );
    }

    /// <summary>
    /// A retry policy that can be applied to synchronous delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public class RetryPolicy<TResult> : Policy<TResult>, IRetryPolicy<TResult>
    {
        private readonly IOnRetryCallback<TResult> _onRetry;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

        internal RetryPolicy(
            PolicyBuilder<TResult> policyBuilder,
            IOnRetryCallback<TResult> onRetry,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => RetryEngine.Implementation(
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