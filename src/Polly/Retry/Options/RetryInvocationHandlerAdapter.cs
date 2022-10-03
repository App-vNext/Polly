using System;

namespace Polly.Retry.Options;

internal class RetryInvocationHandlerAdapter<TResult> : RetryInvocationHandlerBase<TResult>
{
    private readonly RetryInvocationHandlerBase _retryInvocationHandler;

    public RetryInvocationHandlerAdapter(RetryInvocationHandlerBase onRetryCallback)
    {
        _retryInvocationHandler = onRetryCallback ?? throw new ArgumentNullException(nameof(onRetryCallback));
    }

    public override void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context)
        => _retryInvocationHandler.OnRetry(outcome.Exception, waitUntilNextRetry, iteration, context);
}