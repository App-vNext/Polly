using System;

namespace Polly.Retry.Settings;

internal class OnRetryCallbackAdapter<TResult> : IOnRetryCallback<TResult>
{
    private readonly IOnRetryCallback _onRetryCallback;

    public OnRetryCallbackAdapter(IOnRetryCallback onRetryCallback)
    {
        _onRetryCallback = onRetryCallback ?? throw new ArgumentNullException(nameof(onRetryCallback));
    }
    
    public void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context)
    {
        _onRetryCallback.OnRetry(outcome.Exception, waitUntilNextRetry, iteration, context);
    }
}