using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for events that use <see cref="Outcome{TResult}"/> and <typeparamref name="TArgs"/> in the registered event callbacks.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the event uses.</typeparam>
/// <typeparam name="TResult">The result type that this event handles.</typeparam>
public sealed class OutcomeEvent<TArgs, TResult>
    where TArgs : IResilienceArguments
{
    private readonly List<Func<Outcome<TResult>, TArgs, ValueTask>> _callbacks = new();

    /// <summary>
    /// Gets a value indicating whether the event is empty.
    /// </summary>
    internal bool IsEmpty => _callbacks.Count == 0;

    /// <summary>
    /// Registers a callback for the specified result type.
    /// </summary>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public OutcomeEvent<TArgs, TResult> Register(Action callback)
    {
        Guard.NotNull(callback);

        return Register((_, _) =>
        {
            callback();
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for the specified result type.
    /// </summary>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public OutcomeEvent<TArgs, TResult> Register(Action<Outcome<TResult>> callback)
    {
        Guard.NotNull(callback);

        return Register((outcome, _) =>
        {
            callback(outcome);
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for the specified result type.
    /// </summary>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public OutcomeEvent<TArgs, TResult> Register(Action<Outcome<TResult>, TArgs> callback)
    {
        Guard.NotNull(callback);

        return Register((outcome, args) =>
        {
            callback(outcome, args);
            return default;
        });
    }

    /// <summary>
    /// Registers a callback for the specified result type.
    /// </summary>
    /// <param name="callback">The event callback associated with the result type.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public OutcomeEvent<TArgs, TResult> Register(Func<Outcome<TResult>, TArgs, ValueTask> callback)
    {
        Guard.NotNull(callback);

        _callbacks.Add(callback);

        return this;
    }

    /// <summary>
    /// Creates a handler that invokes the registered event callbacks.
    /// </summary>
    /// <returns>Handler instance or <see langword="null"/> if no callbacks are registered.</returns>
    public Func<Outcome<TResult>, TArgs, ValueTask>? CreateHandler()
    {
        return _callbacks.Count switch
        {
            0 => null,
            1 => _callbacks[0],
            _ => CreateHandler(_callbacks.ToArray())
        };
    }

    private static Func<Outcome<TResult>, TArgs, ValueTask> CreateHandler(Func<Outcome<TResult>, TArgs, ValueTask>[] callbacks)
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
