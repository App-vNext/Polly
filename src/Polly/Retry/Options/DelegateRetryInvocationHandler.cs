using System;

namespace Polly.Retry.Options;

public class DelegateRetryInvocationHandler : RetryInvocationHandlerBase
{
    private readonly Action<Exception, TimeSpan, int, Context> _onRetry;

    public DelegateRetryInvocationHandler(Action<Exception> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, _, _, _) => onRetry.Invoke(ex);
    }

    public DelegateRetryInvocationHandler(Action<Exception, int> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, _, iteration, _) => onRetry.Invoke(ex, iteration);
    }

    public DelegateRetryInvocationHandler(Action<Exception, TimeSpan> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, waitUntilNextRetry, _, _) => onRetry.Invoke(ex, waitUntilNextRetry);
    }

    public DelegateRetryInvocationHandler(Action<Exception, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, _, _, context) => onRetry.Invoke(ex, context);
    }

    public DelegateRetryInvocationHandler(Action<Exception, int, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, _, iteration, context) => onRetry.Invoke(ex, iteration, context);
    }

    public DelegateRetryInvocationHandler(Action<Exception, TimeSpan, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, waitUntilNextRetry, _, context) => onRetry.Invoke(ex, waitUntilNextRetry, context);
    }

    public DelegateRetryInvocationHandler(Action<Exception, int, TimeSpan, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (ex, waitUntilNextRetry, iteration, context) =>
            onRetry.Invoke(ex, iteration, waitUntilNextRetry, context);
    }

    public DelegateRetryInvocationHandler(Action<Exception, TimeSpan, int, Context> onRetry)
    {
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    public override void OnRetry(Exception ex, TimeSpan waitUntilNextRetry, int iteration, Context context)
    {
        _onRetry.Invoke(ex, waitUntilNextRetry, iteration, context);
    }
}

public class DelegateRetryInvocationHandler<TResult> : RetryInvocationHandlerBase<TResult>
{
    private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, _, _, _) => onRetry.Invoke(outcome);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, int> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, _, iteration, _) => onRetry.Invoke(outcome, iteration);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, waitUntilNextRetry, _, _) => onRetry.Invoke(outcome, waitUntilNextRetry);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, _, _, context) => onRetry.Invoke(outcome, context);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, _, iteration, context) => onRetry.Invoke(outcome, iteration, context);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, waitUntilNextRetry, _, context) => onRetry.Invoke(outcome, waitUntilNextRetry, context);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
    {
        if (onRetry is null) throw new ArgumentNullException(nameof(onRetry));
        _onRetry = (outcome, waitUntilNextRetry, iteration, context) =>
            onRetry.Invoke(outcome, iteration, waitUntilNextRetry, context);
    }

    public DelegateRetryInvocationHandler(Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    public override void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context)
    {
        _onRetry.Invoke(outcome, waitUntilNextRetry, iteration, context);
    }
}