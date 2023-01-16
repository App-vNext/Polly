namespace Polly.Timeout;

/// <summary>
/// A timeout policy which can be applied to delegates.
/// </summary>
public class TimeoutPolicy : Policy, ITimeoutPolicy
{
    private Func<Context, TimeSpan> _timeoutProvider;
    private TimeoutStrategy _timeoutStrategy;
    private Action<Context, TimeSpan, Task, Exception> _onTimeout;

    internal TimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        _timeoutProvider = Guard.NotNull(timeoutProvider);
        _timeoutStrategy = timeoutStrategy;
        _onTimeout = Guard.NotNull(onTimeout);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        TimeoutEngine.Implementation(
            action,
            context,
            cancellationToken,
            _timeoutProvider,
            _timeoutStrategy,
            _onTimeout);
}

/// <summary>
/// A timeout policy which can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
public class TimeoutPolicy<TResult> : Policy<TResult>, ITimeoutPolicy<TResult>
{
    private Func<Context, TimeSpan> _timeoutProvider;
    private TimeoutStrategy _timeoutStrategy;
    private Action<Context, TimeSpan, Task, Exception> _onTimeout;

    internal TimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        _timeoutProvider = Guard.NotNull(timeoutProvider);
        _timeoutStrategy = timeoutStrategy;
        _onTimeout = Guard.NotNull(onTimeout);
    }

    /// <inheritdoc/>
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        TimeoutEngine.Implementation(
            action,
            context,
            cancellationToken,
            _timeoutProvider,
            _timeoutStrategy,
            _onTimeout);
}
