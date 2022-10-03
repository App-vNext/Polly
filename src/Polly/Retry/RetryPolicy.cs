#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using Polly.Retry.Options;

namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to synchronous delegates.
    /// </summary>
    public class RetryPolicy : Policy, IRetryPolicy
    {
        private readonly RetryInvocationHandlerBase? _retryInvocationHandler;
        private readonly RetryCountValue _permittedRetryCount;
        private readonly SleepDurationProviderBase? _sleepDurationProvider;

        internal RetryPolicy(PolicyBuilder policyBuilder, RetryPolicyOptions options) : base(policyBuilder)
        {
            _permittedRetryCount = options.PermittedRetryCount;
            _sleepDurationProvider = options.SleepDurationProvider;
            _retryInvocationHandler = options.RetryInvocationHandler;
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => RetryEngine.Implementation(
                    action, 
                    context, 
                    cancellationToken,
                    ExceptionPredicates,
                    ResultPredicates<TResult>.None, 
                    new RetryInvocationHandlerAdapter<TResult>(_retryInvocationHandler),
                    _permittedRetryCount,
                    _sleepDurationProvider is not null ? 
                        new SleepDurationProviderAdapter<TResult>(_sleepDurationProvider) : default
                );
    }

    /// <summary>
    /// A retry policy that can be applied to synchronous delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public class RetryPolicy<TResult> : Policy<TResult>, IRetryPolicy<TResult>
    {
        private readonly RetryInvocationHandlerBase<TResult>? _retryInvocationHandler;
        private readonly RetryCountValue _permittedRetryCount;
        private readonly SleepDurationProviderBase<TResult>? _sleepDurationProvider;

        internal RetryPolicy(PolicyBuilder<TResult> policyBuilder, RetryPolicyOptions<TResult> options)
            : base(policyBuilder)
        {
            _permittedRetryCount = options.PermittedRetryCount;
            _sleepDurationProvider = options.SleepDurationProvider;
            _retryInvocationHandler = options.RetryInvocationHandler;
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
                _retryInvocationHandler,
                _permittedRetryCount,
                _sleepDurationProvider
            );
    }
}