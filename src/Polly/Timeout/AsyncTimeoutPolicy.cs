namespace Polly.Timeout;

/// <summary>
/// A timeout policy which can be applied to async delegates.
/// </summary>
public class AsyncTimeoutPolicy : AsyncPolicy, ITimeoutPolicy
{
    private readonly Func<Context, TimeSpan> _timeoutProvider;
    private readonly TimeoutStrategy _timeoutStrategy;
    private readonly Func<Context, TimeSpan, Task, Exception, Task> _onTimeoutAsync;

    internal AsyncTimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
    {
        _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        _timeoutStrategy = timeoutStrategy;
        _onTimeoutAsync = onTimeoutAsync ?? throw new ArgumentNullException(nameof(onTimeoutAsync));
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return AsyncTimeoutEngine.ImplementationAsync(
            action,
            context,
            _timeoutProvider,
            _timeoutStrategy,
            _onTimeoutAsync,
            continueOnCapturedContext,
            cancellationToken);
    }
}

/// <summary>
/// A timeout policy which can be applied to async delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
public class AsyncTimeoutPolicy<TResult> : AsyncPolicy<TResult>, ITimeoutPolicy<TResult>
{
    private readonly TimeoutStrategy _timeoutStrategy;
    private readonly Func<Context, TimeSpan> _timeoutProvider;
    private readonly Func<Context, TimeSpan, Task, Exception, Task> _onTimeoutAsync;

    internal AsyncTimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
    {
        _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        _timeoutStrategy = timeoutStrategy;
        _onTimeoutAsync = onTimeoutAsync ?? throw new ArgumentNullException(nameof(onTimeoutAsync));
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return AsyncTimeoutEngine.ImplementationAsync(
            action,
            context,
            _timeoutProvider,
            _timeoutStrategy,
            _onTimeoutAsync,
            continueOnCapturedContext,
            cancellationToken);
    }
}
