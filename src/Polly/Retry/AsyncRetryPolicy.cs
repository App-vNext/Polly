﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to asynchronous delegates.
    /// </summary>
    public class AsyncRetryPolicy : AsyncPolicy, IRetryPolicy
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
            _onRetryAsync = onRetryAsync ?? throw new ArgumentNullException(nameof(onRetryAsync));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncRetryEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<TResult>.None,
                (outcome, timespan, retryCount, ctx) => _onRetryAsync(outcome.Exception, timespan, retryCount, ctx),
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
    /// A retry policy that can be applied to asynchronous delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public class AsyncRetryPolicy<TResult> : AsyncPolicy<TResult>, IRetryPolicy<TResult>
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
            _onRetryAsync = onRetryAsync ?? throw new ArgumentNullException(nameof(onRetryAsync));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncRetryEngine.ImplementationAsync(
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

