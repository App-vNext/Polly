using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// Predicate that uses <see cref="Outcome{TResult}"/> and <typeparamref name="TResult"/> as an input.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the predicate uses.</typeparam>
/// <typeparam name="TResult">The result type that this predicate handles.</typeparam>
public sealed class OutcomePredicate<TArgs, TResult>
    where TArgs : IResilienceArguments
{
    private readonly List<Func<Outcome<TResult>, TArgs, ValueTask<bool>>> _predicates = new();

    /// <summary>
    /// Gets a value indicating whether the predicate is empty.
    /// </summary>
    public bool IsEmpty => _predicates.Count == 0;

    /// <summary>
    /// Adds an exception predicate for the specified exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleException<TException>()
        where TException : Exception
    {
        return HandleOutcome((outcome, _) =>
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
    public OutcomePredicate<TArgs, TResult> HandleException<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, _) =>
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
    public OutcomePredicate<TArgs, TResult> HandleException<TException>(Func<TException, TArgs, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, args) =>
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
    public OutcomePredicate<TArgs, TResult> HandleException<TException>(Func<TException, TArgs, ValueTask<bool>> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, args) =>
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
    /// <param name="value">The result value to be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <remarks>
    /// By default, the default equality comparer is used to compare the result value with the value specified in this method.
    /// </remarks>
    public OutcomePredicate<TArgs, TResult> HandleResult(TResult value)
    {
        return HandleResult((result, _) => new ValueTask<bool>(EqualityComparer<TResult>.Default.Equals(result!, value)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleResult(Func<TResult?, bool> predicate)
    {
        Guard.NotNull(predicate);

        return HandleResult((result, _) => new ValueTask<bool>(predicate(result)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleResult(Func<TResult?, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, args) =>
        {
            return new ValueTask<bool>(outcome.TryGetResult(out var result) && predicate(result, args));
        });
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleResult(Func<TResult?, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, args) =>
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
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleOutcome(Func<Outcome<TResult>, TArgs, bool> predicate)
    {
        Guard.NotNull(predicate);

        return HandleOutcome((outcome, args) => new ValueTask<bool>(predicate(outcome, args)));
    }

    /// <summary>
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs, TResult> HandleOutcome(Func<Outcome<TResult>, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Creates a handler for the specified predicates.
    /// </summary>
    /// <returns>Handler instance or null if no predicates are registered.</returns>
    public Func<Outcome<TResult>, TArgs, ValueTask<bool>>? CreateHandler()
    {
        var pairs = _predicates.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => _predicates[0],
            _ => CreateHandler(_predicates.ToArray())
        };
    }

    private static Func<Outcome<TResult>, TArgs, ValueTask<bool>> CreateHandler(Func<Outcome<TResult>, TArgs, ValueTask<bool>>[] predicates)
    {
        return async (outcome, args) =>
        {
            foreach (var predicate in predicates)
            {
                if (await predicate(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext))
                {
                    return true;
                }
            }

            return false;
        };
    }
}
