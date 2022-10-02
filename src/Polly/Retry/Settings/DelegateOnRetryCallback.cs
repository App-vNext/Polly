using System;

namespace Polly.Retry.Settings;

public class DelegateOnRetryCallback : IOnRetryCallback
{
    private readonly Action<Exception, TimeSpan, int, Context> _onRetry;

    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, _, _, _) => onRetry.Invoke(ex);
    }
    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, int> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, _, iteration, _) => onRetry.Invoke(ex, iteration);
    }
    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, TimeSpan> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, waitUntilNextRetry, _, _) => onRetry.Invoke(ex, waitUntilNextRetry);
    }
    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, _, _, context) => onRetry.Invoke(ex, context);
    }

    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, int, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, _, iteration, context) => onRetry.Invoke(ex, iteration, context);
    }

    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, TimeSpan, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, waitUntilNextRetry, _, context) => onRetry.Invoke(ex, waitUntilNextRetry, context);
    }

    private static Action<Exception, TimeSpan, int, Context> Wrap(Action<Exception, int, TimeSpan, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (ex, waitUntilNextRetry, iteration, context) => 
            onRetry.Invoke(ex, iteration, waitUntilNextRetry, context);
    }
    
    public DelegateOnRetryCallback(Action<Exception> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<Exception, int> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<Exception, TimeSpan> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<Exception, Context> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<Exception, int, Context> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<Exception, TimeSpan, Context> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<Exception, int, TimeSpan, Context> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<Exception, TimeSpan, int, Context> onRetry)
    {
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    public void OnRetry(Exception ex, TimeSpan waitUntilNextRetry, int iteration, Context context)
    {
        _onRetry.Invoke(ex, waitUntilNextRetry, iteration, context);
    }
}

public class DelegateOnRetryCallback<TResult> : IOnRetryCallback<TResult>
{
    private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;

    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, _, _, _) => onRetry.Invoke(outcome);
    }
    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, int> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, _, iteration, _) => onRetry.Invoke(outcome, iteration);
    }
    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, TimeSpan> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, waitUntilNextRetry, _, _) => onRetry.Invoke(outcome, waitUntilNextRetry);
    }
    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, _, _, context) => onRetry.Invoke(outcome, context);
    }

    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, int, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, _, iteration, context) => onRetry.Invoke(outcome, iteration, context);
    }

    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, TimeSpan, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, waitUntilNextRetry, _, context) => onRetry.Invoke(outcome, waitUntilNextRetry, context);
    }

    private static Action<DelegateResult<TResult>, TimeSpan, int, Context> Wrap(Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry)
    {
        onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
        return (outcome, waitUntilNextRetry, iteration, context) => 
            onRetry.Invoke(outcome, iteration, waitUntilNextRetry, context);
    }
    
    public DelegateOnRetryCallback(Action<DelegateResult<TResult>> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, int> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, TimeSpan> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, Context> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, int, Context> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, TimeSpan, Context> onRetry) : this(Wrap(onRetry)) { }
    
    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, int, TimeSpan, Context> onRetry) : this(Wrap(onRetry)) { }

    public DelegateOnRetryCallback(Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry)
    {
        _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
    }

    public void OnRetry(DelegateResult<TResult> outcome, TimeSpan waitUntilNextRetry, int iteration, Context context) 
        => _onRetry.Invoke(outcome, waitUntilNextRetry, iteration, context);
}