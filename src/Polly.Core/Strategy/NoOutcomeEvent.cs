namespace Polly.Strategy;

/// <summary>
/// This class holds a list of callbacks that are invoked when some event occurs.
/// The callbacks are executed for all result types and do not require <see cref="Outcome{TResult}"/>.
/// </summary>
/// <remarks>This class supports registering multiple event callbacks.
/// The registered callbacks are executed one-by-one in the same order as they were registered.</remarks>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
public sealed class NoOutcomeEvent<TArgs>
    where TArgs : IResilienceArguments
{
    private readonly List<Func<TArgs, ValueTask>> _callbacks = new();

    /// <summary>
    /// Adds an asynchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public NoOutcomeEvent<TArgs> Register(Func<TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Adds a synchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public NoOutcomeEvent<TArgs> Register(Action<TArgs> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(args =>
        {
            callback(args);
            return default;
        });

        return this;
    }

    /// <summary>
    /// Adds a synchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public NoOutcomeEvent<TArgs> Register(Action callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(_ =>
        {
            callback();
            return default;
        });

        return this;
    }

    /// <summary>
    /// Creates a callback handler for all registered event callbacks.
    /// </summary>
    /// <returns>A callback handler.</returns>
    public Func<TArgs, ValueTask>? CreateHandler()
    {
        return _callbacks.Count switch
        {
            0 => null,
            1 => _callbacks[0],
            _ => CreateHandler(_callbacks.ToArray())
        };
    }

    private static Func<TArgs, ValueTask> CreateHandler(Func<TArgs, ValueTask>[] callbacks)
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
