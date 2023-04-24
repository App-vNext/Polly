using System;
using System.Collections.Generic;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// Predicate that uses void-based <see cref="Outcome"/> as an input.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the predicate uses.</typeparam>
public sealed class VoidOutcomePredicate<TArgs>
    where TArgs : IResilienceArguments
{
    private readonly List<Func<Outcome, TArgs, ValueTask<bool>>> _predicates = new();

    /// <summary>
    /// Gets a value indicating whether the predicate is empty.
    /// </summary>
    public bool IsEmpty => _predicates.Count == 0;

    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomePredicate<TArgs> HandleException<TException>()
        where TException : Exception
    {
        return HandleOutcome((outcome, _) =>
        {
            if (outcome.Exception is TException)
            {
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        });
    }

    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomePredicate<TArgs> HandleException<TException>(Func<TException, bool> predicate)
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
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomePredicate<TArgs> HandleException<TException>(Func<TException, TArgs, bool> predicate)
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
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomePredicate<TArgs> HandleException<TException>(Func<TException, TArgs, ValueTask<bool>> predicate)
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
    /// Adds a result predicate for the specified result type.
    /// </summary>
    /// <param name="predicate">The predicate to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    private VoidOutcomePredicate<TArgs> HandleOutcome(Func<Outcome, TArgs, ValueTask<bool>> predicate)
    {
        Guard.NotNull(predicate);

        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Creates a handler for the specified predicates.
    /// </summary>
    /// <returns>Handler instance or null if no predicates are registered.</returns>
    public Func<Outcome, TArgs, ValueTask<bool>>? CreateHandler()
    {
        var pairs = _predicates.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => _predicates[0],
            _ => CreateHandler(_predicates.ToArray())
        };
    }

    private static Func<Outcome, TArgs, ValueTask<bool>> CreateHandler(Func<Outcome, TArgs, ValueTask<bool>>[] predicates)
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
