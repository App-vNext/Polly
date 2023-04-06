namespace Polly.Strategy;

/// <summary>
/// This class holds a list of callbacks that are invoked when some event occurs.
/// The callbacks are executed for all result types and do not require <see cref="Outcome{TResult}"/>.
/// </summary>
/// <remarks>This class supports registering multiple event callbacks.
/// The registered callbacks are executed one-by-one in the same order as they were registered.</remarks>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
/// <typeparam name="TSelf">The class that implements <see cref="SimpleEvent{TArgs, TSelf}"/>.</typeparam>
public abstract class SimpleEvent<TArgs, TSelf>
    where TArgs : IResilienceArguments
    where TSelf : SimpleEvent<TArgs, TSelf>
{
    private readonly List<Func<TArgs, ValueTask>> _callbacks = new();

    /// <summary>
    /// Adds an asynchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    public TSelf Add(Func<TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(callback);
        return (TSelf)this;
    }

    /// <summary>
    /// Adds a synchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    public TSelf Add(Action<TArgs> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(args =>
        {
            callback(args);
            return default;
        });

        return (TSelf)this;
    }

    /// <summary>
    /// Adds a synchronous event callback.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    /// <returns>This instance.</returns>
    public TSelf Add(Action callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(_ =>
        {
            callback();
            return default;
        });

        return (TSelf)this;
    }

    internal Func<TArgs, ValueTask>? CreateHandler()
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
