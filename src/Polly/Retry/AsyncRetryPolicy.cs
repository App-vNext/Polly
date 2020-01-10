using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to asynchronous executions.
    /// </summary>
    public class AsyncRetryPolicy : AsyncPolicyV8, IAsyncRetryPolicy
    {
        private readonly Func<Exception, TimeSpan, int, Context, Task> _onRetryAsync;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, Exception, Context, TimeSpan> _sleepDurationProvider;

        internal AsyncRetryPolicy(
            PolicyBuilder policyBuilder,
            Func<Exception, TimeSpan, int, Context, Task> onRetryAsync,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncRetryEngineV8.ImplementationAsync(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<TResult>.None,
                _onRetryAsync == null ? (Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>)null : (outcome, timespan, retryCount, ctx) => _onRetryAsync(outcome.Exception, timespan, retryCount, ctx),
                _permittedRetryCount,
                _sleepDurationsEnumerable,
                _sleepDurationProvider != null 
                    ? (retryCount, outcome, ctx) =>  _sleepDurationProvider(retryCount, outcome.Exception, ctx) 
                    : (Func<int, DelegateResult<TResult>, Context, TimeSpan>)null,
                continueOnCapturedContext
            );
        }
    }

    /// <summary>
    /// A retry policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncRetryPolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncRetryPolicy<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> _onRetryAsync;
        private readonly int _permittedRetryCount;
        private readonly IEnumerable<TimeSpan> _sleepDurationsEnumerable;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

        internal AsyncRetryPolicy(
            PolicyBuilder<TResult> policyBuilder,
            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync,
            int permittedRetryCount = Int32.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null
        )
            : base(policyBuilder)
        {
            _permittedRetryCount = permittedRetryCount;
            _sleepDurationsEnumerable = sleepDurationsEnumerable;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncRetryEngineV8.ImplementationAsync(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _onRetryAsync,
                _permittedRetryCount,
                _sleepDurationsEnumerable,
                _sleepDurationProvider,
                continueOnCapturedContext
            );
    }
}

