using System;

namespace Polly.Retry.Settings;

public interface IOnRetryCallback
{
    void OnRetry(Exception ex, TimeSpan waitUntilNextRetry, int iteration, Context context);
}

public interface IOnRetryCallback<TResult>
{
    void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context);
}