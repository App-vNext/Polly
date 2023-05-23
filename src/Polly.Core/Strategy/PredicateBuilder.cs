using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// Defines a builder for creating predicates for <typeparamref name="TResult"/> and <see cref="Exception"/> combinations.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class PredicateBuilder<TResult>
{
    private readonly List<Predicate<Outcome<TResult>>> _predicates = new();

    /// <summary>
    /// Adds a predicate for handling exceptions of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> Handle<TException>()
        where TException : Exception
    {
        return Handle<TException>(_ => true);
    }

    /// <summary>
    /// Adds a predicate for handling exceptions of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <param name="predicate">The predicate function to use for handling the exception.</param>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public PredicateBuilder<TResult> Handle<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return Add(outcome => outcome.Exception is TException exception && predicate(exception));
    }

    /// <summary>
    /// Adds a predicate for handling inner exceptions of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the inner exception to handle.</typeparam>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> HandleInner<TException>()
        where TException : Exception
    {
        return HandleInner<TException>(_ => true);
    }

    /// <summary>
    /// Adds a predicate for handling inner exceptions of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the inner exception to handle.</typeparam>
    /// <param name="predicate">The predicate function to use for handling the inner exception.</param>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public PredicateBuilder<TResult> HandleInner<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return Add(outcome => outcome.Exception?.InnerException is TException exception && predicate(exception));
    }

    /// <summary>
    /// Adds a predicate for handling results.
    /// </summary>
    /// <param name="predicate">The predicate function to use for handling the result.</param>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> HandleResult(Func<TResult, bool> predicate)
        => Add(outcome => outcome.TryGetResult(out var result) && predicate(result!));

    /// <summary>
    /// Adds a predicate for handling results with a specific value.
    /// </summary>
    /// <param name="result">The result value to handle.</param>
    /// <param name="comparer">The comparer to use for comparing results. If null, the default comparer is used.</param>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> HandleResult(TResult result, IEqualityComparer<TResult>? comparer = null)
    {
        comparer ??= EqualityComparer<TResult>.Default;

        return HandleResult(r => comparer.Equals(r, result));
    }

    internal Func<Outcome<TResult>, TArgs, ValueTask<bool>> CreatePredicate<TArgs>() => _predicates.Count switch
    {
        0 => throw new ValidationException("No predicates were configured. There must be at least one predicate added."),
        1 => CreatePredicate<TArgs>(_predicates[0]),
        _ => CreatePredicate<TArgs>(_predicates.ToArray()),
    };

    private static Func<Outcome<TResult>, TArgs, ValueTask<bool>> CreatePredicate<TArgs>(Predicate<Outcome<TResult>> predicate)
        => (outcome, _) => new ValueTask<bool>(predicate(outcome));

    private static Func<Outcome<TResult>, TArgs, ValueTask<bool>> CreatePredicate<TArgs>(Predicate<Outcome<TResult>>[] predicates)
    {
        return (outcome, _) =>
        {
            foreach (var predicate in predicates)
            {
                if (predicate(outcome))
                {
                    return new ValueTask<bool>(true);
                }
            }

            return new ValueTask<bool>(false);
        };
    }

    private PredicateBuilder<TResult> Add(Predicate<Outcome<TResult>> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }
}
