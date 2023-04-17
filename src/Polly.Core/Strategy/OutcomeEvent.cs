using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for events that use <see cref="Outcome{TResult}"/> and <typeparamref name="TArgs"/> in the registered event callbacks.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
public sealed partial class OutcomeEvent<TArgs>
    where TArgs : IResilienceArguments
{
    private readonly Dictionary<Type, (object callback, Func<object?> handlerFactory)> _callbacks = new();

    /// <summary>
    /// Gets a value indicating whether the event is empty.
    /// </summary>
    public bool IsEmpty => _callbacks.Count == 0;

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> Register<TResult>(Action callback)
    {
        Guard.NotNull(callback);

        return ConfigureCallbacks<TResult>(c => c.Register(callback));
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> Register<TResult>(Action<Outcome<TResult>> callback)
    {
        Guard.NotNull(callback);

        return ConfigureCallbacks<TResult>(c => c.Register(callback));
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> Register<TResult>(Action<Outcome<TResult>, TArgs> callback)
    {
        Guard.NotNull(callback);

        return ConfigureCallbacks<TResult>(c => c.Register(callback));
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> Register<TResult>(Func<Outcome<TResult>, TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        return ConfigureCallbacks<TResult>(c => c.Register(callback));
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callbacks for.</typeparam>
    /// <param name="configure">Action that configures the callbacks.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> ConfigureCallbacks<TResult>(Action<OutcomeEvent<TArgs, TResult>> configure)
    {
        Guard.NotNull(configure);

        if (!_callbacks.ContainsKey(typeof(TResult)))
        {
            SetCallbacks(new OutcomeEvent<TArgs, TResult>());
        }

        configure((OutcomeEvent<TArgs, TResult>)_callbacks[typeof(TResult)].callback);
        return this;
    }

    /// <summary>
    /// Sets a callbacks for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callbacks for.</typeparam>
    /// <param name="callbacks">The callbacks instance.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeEvent<TArgs> SetCallbacks<TResult>(OutcomeEvent<TArgs, TResult> callbacks)
    {
        Guard.NotNull(callbacks);

        _callbacks[typeof(TResult)] = (callbacks, callbacks.CreateHandler);
        return this;
    }

    /// <summary>
    /// Creates a handler that invokes the registered event callbacks.
    /// </summary>
    /// <returns>Handler instance or <c>null</c> if no callbacks are registered.</returns>
    public Handler? CreateHandler()
    {
        var pairs = _callbacks
             .Select(pair => new KeyValuePair<Type, object?>(pair.Key, pair.Value.handlerFactory()))
             .Where(pair => pair.Value != null)
             .ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value!),
            _ => new TypesHandler(pairs!)
        };
    }
}
