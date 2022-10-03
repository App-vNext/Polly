using System;

namespace Polly.Retry.Options;

public abstract class RetryInvocationHandlerBase
{
    public abstract void OnRetry(Exception ex, TimeSpan waitUntilNextRetry, int iteration, Context context);
}

public abstract class RetryInvocationHandlerBase<TResult>
{
    public abstract void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context);
}