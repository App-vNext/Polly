using System;
using System.Collections.Generic;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for predicates that use <see cref="Strategy.Outcome{TResult}"/> as an input.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the predicate uses.</typeparam>
public sealed partial class OutcomePredicate<TArgs>
    where TArgs : IResilienceArguments
{
    private readonly Dictionary<Type, (object predicate, Func<object?> handlerFactory)> _predicates = new();

    /// <summary>
    /// Gets a value indicating whether the predicate is empty.
    /// </summary>
    internal bool IsEmpty => _predicates.Count == 0;

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs> HandleException<TException>()
        where TException : Exception
    {
        return ConfigurePredicates<ExceptionOutcome>(p => p.HandleException<TException>());
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleException<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<ExceptionOutcome>(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleException<TException>(Func<TException, TArgs, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<ExceptionOutcome>(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleException<TException>(Func<TException, TArgs, ValueTask<bool>> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<ExceptionOutcome>(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result value.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="value">The result value to be retried.</param>
    /// <param name="comparer">The comparer to use. If null the default comparer for the type will be used.</param>
    /// <returns>The current updated instance.</returns>
    /// <remarks>
    /// By default, the default equality comparer is used to compare the result value with the value specified in this method.
    /// </remarks>
    public OutcomePredicate<TArgs> HandleResult<TResult>(TResult value, IEqualityComparer<TResult>? comparer = null)
    {
        return ConfigurePredicates<TResult>(p => p.HandleResult(value, comparer));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleResult<TResult>(Func<TResult?, bool> predicate)
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<TResult>(p => p.HandleResult(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleResult<TResult>(Func<TResult?, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<TResult>(p => p.HandleResult(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleResult<TResult>(Func<TResult?, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<TResult>(p => p.HandleResult(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleOutcome<TResult>(Func<Outcome<TResult>, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<TResult>(p => p.HandleOutcome(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleOutcome<TResult>(Func<Outcome<TResult>, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        return ConfigurePredicates<TResult>(p => p.HandleOutcome(predicate));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="configure">Callback that configures a result predicate.</param>
    /// <returns>The current updated instance.</returns>
    private OutcomePredicate<TArgs> ConfigurePredicates<TResult>(Action<OutcomePredicate<TArgs, TResult>> configure)
    {
        Guard.NotNull(configure);

        if (!_predicates.TryGetValue(typeof(TResult), out var predicate))
        {
            SetPredicates(new OutcomePredicate<TArgs, TResult>());
            predicate = _predicates[typeof(TResult)];
        }

        configure((OutcomePredicate<TArgs, TResult>)predicate.predicate);
        return this;
    }

    /// <summary>
    /// Sets the predicates for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicates">The configured predicates.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicates"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> SetPredicates<TResult>(OutcomePredicate<TArgs, TResult> predicates)
    {
        Guard.NotNull(predicates);
        _predicates[typeof(TResult)] = (predicates, predicates.CreateHandler);
        return this;
    }

    /// <summary>
    /// Creates a handler for the specified predicates.
    /// </summary>
    /// <returns>Handler instance or null if no predicates are registered.</returns>
    public Handler? CreateHandler()
    {
        var pairs = _predicates
            .Select(pair => new KeyValuePair<Type, object?>(pair.Key, pair.Value.handlerFactory()))
            .Where(pair => pair.Value is not null)
            .ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value!),
            _ => new TypesHandler(pairs!)
        };
    }

    /// <summary>
    /// A special type for predicates that only care about exceptions.
    /// </summary>
    private sealed class ExceptionOutcome
    {
    }
}
