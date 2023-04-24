using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// Class for void-based events that use <see cref="Outcome"/> and <typeparamref name="TArgs"/> in the registered event callbacks.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
public sealed class VoidOutcomeEvent<TArgs>
    where TArgs : IResilienceArguments
{
    private readonly List<Func<Outcome, TArgs, ValueTask>> _callbacks = new();

    /// <summary>
    /// Gets a value indicating whether the event is empty.
    /// </summary>
    public bool IsEmpty => _callbacks.Count == 0;

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeEvent<TArgs> Register(Action callback)
    {
        Guard.NotNull(callback);

        return Register((_, _) =>
        {
            callback();
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeEvent<TArgs> Register(Action<Outcome> callback)
    {
        Guard.NotNull(callback);

        return Register((outcome, _) =>
        {
            callback(outcome);
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeEvent<TArgs> Register(Action<Outcome, TArgs> callback)
    {
        Guard.NotNull(callback);

        return Register((outcome, args) =>
        {
            callback(outcome, args);
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for void-based results.
    /// </summary>
    /// <param name="callback">The event callback associated with the void result type.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeEvent<TArgs> Register(Func<Outcome, TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(callback);

        return this;
    }

    /// <summary>
    /// Creates a handler that invokes the registered event callbacks.
    /// </summary>
    /// <returns>Handler instance or <c>null</c> if no callbacks are registered.</returns>
    public Func<Outcome, TArgs, ValueTask>? CreateHandler()
    {
        return _callbacks.Count switch
        {
            0 => null,
            1 => _callbacks[0],
            _ => CreateHandler(_callbacks.ToArray())
        };
    }

    private static Func<Outcome, TArgs, ValueTask> CreateHandler(Func<Outcome, TArgs, ValueTask>[] callbacks)
    {
        return async (outcome, args) =>
        {
            foreach (var predicate in callbacks)
            {
                await predicate(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
        };
    }
}
