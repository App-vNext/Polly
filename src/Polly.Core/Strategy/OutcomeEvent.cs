using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for events that use <see cref="Outcome{TResult}"/> and <typeparamref name="TArgs"/> in the registered event callbacks.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
/// <typeparam name="TSelf">The class that implements <see cref="OutcomeEvent{TArgs, TSelf}"/>.</typeparam>
public abstract partial class OutcomeEvent<TArgs, TSelf>
    where TArgs : IResilienceArguments
    where TSelf : OutcomeEvent<TArgs, TSelf>
{
    private readonly Dictionary<Type, List<object>> _callbacks = new();

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
    public TSelf Add<TResult>(Action callback)
    {
        Guard.NotNull(callback);

        return Add<TResult>((_, _) =>
        {
            callback();
            return default;
        });
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Add<TResult>(Action<Outcome<TResult>> callback)
    {
        Guard.NotNull(callback);

        return Add<TResult>((outcome, _) =>
        {
            callback(outcome);
            return default;
        });
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Add<TResult>(Action<Outcome<TResult>, TArgs> callback)
    {
        Guard.NotNull(callback);

        return Add<TResult>((outcome, args) =>
        {
            callback(outcome, args);
            return default;
        });
    }

    /// <summary>
    /// Adds a callback for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a callback for.</typeparam>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Add<TResult>(Func<Outcome<TResult>, TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        if (!_callbacks.TryGetValue(typeof(TResult), out var callbacks))
        {
            callbacks = new List<object> { callback };
            _callbacks.Add(typeof(TResult), callbacks);
        }
        else
        {
            callbacks.Add(callback);
        }

        return (TSelf)this;
    }

    /// <summary>
    /// Creates a handler that invokes the registered event callbacks.
    /// </summary>
    /// <returns>Handler instance or <c>null</c> if no callbacks are registered.</returns>
    protected internal Handler? CreateHandler()
    {
        var pairs = _callbacks.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value),
            _ => new TypesHandler(pairs)
        };
    }
}
