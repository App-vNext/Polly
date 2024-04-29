namespace Polly.Timeout;

/// <summary>
/// A timeout policy which can be applied to delegates.
/// </summary>
public class TimeoutPolicy : Policy, ITimeoutPolicy
{
    private readonly TimeoutStrategy _timeoutStrategy;
    private readonly Func<Context, TimeSpan> _timeoutProvider;
    private readonly Action<Context, TimeSpan, Task, Exception> _onTimeout;

    internal TimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        _timeoutStrategy = timeoutStrategy;
        _onTimeout = onTimeout ?? throw new ArgumentNullException(nameof(onTimeout));
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
/// <typeparam name="TResult">The type of the result.</typeparam>
public class TimeoutPolicy<TResult> : Policy<TResult>, ITimeoutPolicy<TResult>
{
    private readonly TimeoutStrategy _timeoutStrategy;
    private readonly Func<Context, TimeSpan> _timeoutProvider;
    private readonly Action<Context, TimeSpan, Task, Exception> _onTimeout;

    internal TimeoutPolicy(
        Func<Context, TimeSpan> timeoutProvider,
        TimeoutStrategy timeoutStrategy,
        Action<Context, TimeSpan, Task, Exception> onTimeout)
    {
        _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
        _timeoutStrategy = timeoutStrategy;
        _onTimeout = onTimeout ?? throw new ArgumentNullException(nameof(onTimeout));
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
