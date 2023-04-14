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
    public OutcomeEvent<TArgs> Register<TResult>(Action callback)
    {
        Guard.NotNull(callback);

        return Register<TResult>((_, _) =>
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
    public OutcomeEvent<TArgs> Register<TResult>(Action<Outcome<TResult>> callback)
    {
        Guard.NotNull(callback);

        return Register<TResult>((outcome, _) =>
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
    public OutcomeEvent<TArgs> Register<TResult>(Action<Outcome<TResult>, TArgs> callback)
    {
        Guard.NotNull(callback);

        return Register<TResult>((outcome, args) =>
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
    public OutcomeEvent<TArgs> Register<TResult>(Func<Outcome<TResult>, TArgs, ValueTask> callback)
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

        return this;
    }

    /// <summary>
    /// Creates a handler that invokes the registered event callbacks.
    /// </summary>
    /// <returns>Handler instance or <c>null</c> if no callbacks are registered.</returns>
    public Handler? CreateHandler()
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
