using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for predicates that use <see cref="Strategy.Outcome{TResult}"/> as an input.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the predicate uses.</typeparam>
/// <typeparam name="TSelf">The class that implements <see cref="OutcomePredicate{TArgs, TSelf}"/>.</typeparam>
public abstract partial class OutcomePredicate<TArgs, TSelf>
    where TArgs : IResilienceArguments
    where TSelf : OutcomePredicate<TArgs, TSelf>
{
    private readonly Dictionary<Type, List<object>> _predicates = new();

    /// <summary>
    /// Gets a value indicating whether the predicate is empty.
    /// </summary>
    public bool IsEmpty => _predicates.Count == 0;

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <returns>The current updated instance.</returns>
    public TSelf Exception<TException>()
        where TException : Exception
    {
        return Outcome<ExceptionOutcome>((outcome, _) =>
        {
            if (outcome.Exception is TException typedException)
            {
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Exception<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return Outcome<ExceptionOutcome>((outcome, _) =>
        {
            if (outcome.Exception is TException typedException)
            {
                return new ValueTask<bool>(predicate(typedException));
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Exception<TException>(Func<TException, TArgs, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return Outcome<ExceptionOutcome>((outcome, args) =>
        {
            if (outcome.Exception is TException typedException)
            {
                return new ValueTask<bool>(predicate(typedException, args));
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Exception<TException>(Func<TException, TArgs, ValueTask<bool>> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return Outcome<ExceptionOutcome>((outcome, args) =>
        {
            if (outcome.Exception is TException typedException)
            {
                return predicate(typedException, args);
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds a result predicate for the specified result value.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="value">The result value to be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <remarks>
    /// By default, the default equality comparer is used to compare the result value with the value specified in this method.
    /// </remarks>
    public TSelf Result<TResult>(TResult value)
    {
        return Result<TResult>((result, _) => new ValueTask<bool>(EqualityComparer<TResult>.Default.Equals(result, value)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Result<TResult>(Func<TResult, bool> predicate)
    {
        Guard.NotNull(predicate);

        return Result<TResult>((result, _) => new ValueTask<bool>(predicate(result)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Result<TResult>(Func<TResult, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return Outcome<TResult>((outcome, args) =>
        {
            return new ValueTask<bool>(outcome.TryGetResult(out var result) && predicate(result, args));
        });
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Result<TResult>(Func<TResult, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        return Outcome<TResult>((outcome, args) =>
        {
            if (outcome.TryGetResult(out var result))
            {
                return predicate(result, args);
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Outcome<TResult>(Func<Outcome<TResult>, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return Outcome<TResult>((outcome, args) => new ValueTask<bool>(predicate(outcome, args)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf Outcome<TResult>(Func<Outcome<TResult>, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        if (!_predicates.TryGetValue(typeof(TResult), out var predicates))
        {
            predicates = new List<object> { predicate };
            _predicates.Add(typeof(TResult), predicates);
        }
        else
        {
            predicates.Add(predicate);
        }

        return (TSelf)this;
    }

    /// <summary>
    /// Creates a handler for the specified predicates.
    /// </summary>
    /// <returns>Handler instance or null if no predicates are registered.</returns>
    protected internal Handler? CreateHandler()
    {
        var pairs = _predicates.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value),
            _ => new TypesHandler(pairs)
        };
    }

    /// <summary>
    /// A special type for predicates that only care about exceptions.
    /// </summary>
    private sealed class ExceptionOutcome
    {
    }
}
