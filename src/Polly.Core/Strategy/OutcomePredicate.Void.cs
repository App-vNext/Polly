namespace Polly.Strategy;

public sealed partial class OutcomePredicate<TArgs>
    where TArgs : IResilienceArguments
{
    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <returns>The current updated instance.</returns>
    public OutcomePredicate<TArgs> HandleVoidException<TException>()
        where TException : Exception
    {
        return ConfigureVoidPredicates(p => p.HandleException<TException>());
    }

    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleVoidException<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigureVoidPredicates(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleVoidException<TException>(Func<TException, TArgs, bool> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigureVoidPredicates(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Adds an exception predicate for void-based results.
    /// </summary>
    /// <typeparam name="TException">The exception type to add a predicate for.</typeparam>
    /// <param name="predicate">The predicate to determine if the exception should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> HandleVoidException<TException>(Func<TException, TArgs, ValueTask<bool>> predicate)
        where TException : Exception
    {
        Guard.NotNull(predicate);

        return ConfigureVoidPredicates(p => p.HandleException(predicate));
    }

    /// <summary>
    /// Sets the predicates for the void-based results.
    /// </summary>
    /// <param name="predicates">The configured predicates.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicates"/> is <see langword="null"/>.</exception>
    public OutcomePredicate<TArgs> SetVoidPredicates(VoidOutcomePredicate<TArgs> predicates)
    {
        Guard.NotNull(predicates);
        _predicates[typeof(VoidResult)] = (predicates, predicates.CreateHandler);
        return this;
    }

    /// <summary>
    /// Adds a result predicate for void outcomes.
    /// </summary>
    /// <param name="configure">Callback that configures a result predicate.</param>
    /// <returns>The current updated instance.</returns>
    private OutcomePredicate<TArgs> ConfigureVoidPredicates(Action<VoidOutcomePredicate<TArgs>> configure)
    {
        Guard.NotNull(configure);

        if (!_predicates.TryGetValue(typeof(VoidResult), out var predicate))
        {
            SetVoidPredicates(new VoidOutcomePredicate<TArgs>());
            predicate = _predicates[typeof(VoidResult)];
        }

        configure((VoidOutcomePredicate<TArgs>)predicate.predicate);
        return this;
    }
}
