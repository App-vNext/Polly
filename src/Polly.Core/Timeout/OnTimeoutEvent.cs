namespace Polly.Timeout;

/// <summary>
/// An event that is raised when a timeout occurs.
/// </summary>
/// <remarks>This class supports registering multiple on-timeout callbacks.
/// The registered callbacks are executed one-by-one in the same order as they were registered.</remarks>
public sealed class OnTimeoutEvent
{
    private readonly List<Func<OnTimeoutArguments, ValueTask>> _events = new();

    /// <summary>
    /// Adds an asynchronous on-timeout callback.
    /// </summary>
    /// <param name="onTimeout">The specified callback.</param>
    /// <returns>This instance.</returns>
    public OnTimeoutEvent Add(Func<OnTimeoutArguments, ValueTask> onTimeout)
    {
        Guard.NotNull(onTimeout);

        _events.Add(onTimeout);
        return this;
    }

    /// <summary>
    /// Adds a synchronous on-timeout callback.
    /// </summary>
    /// <param name="onTimeout">The specified callback.</param>
    /// <returns>This instance.</returns>
    public OnTimeoutEvent Add(Action<OnTimeoutArguments> onTimeout)
    {
        Guard.NotNull(onTimeout);

        _events.Add(args =>
        {
            onTimeout(args);
            return default;
        });
        return this;
    }

    /// <summary>
    /// Adds a synchronous on-timeout callback.
    /// </summary>
    /// <param name="onTimeout">The specified callback.</param>
    /// <returns>This instance.</returns>
    public OnTimeoutEvent Add(Action onTimeout)
    {
        Guard.NotNull(onTimeout);

        _events.Add(_ =>
        {
            onTimeout();
            return default;
        });
        return this;
    }

    internal Func<OnTimeoutArguments, ValueTask>? CreateHandler()
    {
        return _events.Count switch
        {
            0 => null,
            1 => _events[0],
            _ => CreateHandler(_events.ToArray())
        };
    }

    private static Func<OnTimeoutArguments, ValueTask> CreateHandler(Func<OnTimeoutArguments, ValueTask>[] callbacks)
    {
        return async args =>
        {
            foreach (var callback in callbacks)
            {
                await callback(args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
        };
    }
}
