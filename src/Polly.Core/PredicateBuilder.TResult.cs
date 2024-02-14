namespace Polly;

/// <summary>
/// Defines a builder for creating predicates for <typeparamref name="TResult"/> and <see cref="Exception"/> combinations.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public partial class PredicateBuilder<TResult>
{
    private readonly List<Predicate<Outcome<TResult>>> _predicates = [];

    /// <summary>
    /// Adds a predicate for handling exceptions of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> Handle<TException>()
        where TException : Exception => Handle<TException>(static _ => true);

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
        where TException : Exception => HandleInner<TException>(static _ => true);

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

        return Add(outcome => outcome.Exception?.InnerException is TException innerException && predicate(innerException));
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
    /// <param name="comparer">The comparer to use for comparing results. If <see langword="null"/> , the default comparer is used.</param>
    /// <returns>The same instance of the <see cref="PredicateBuilder{TResult}"/> for chaining.</returns>
    public PredicateBuilder<TResult> HandleResult(TResult result, IEqualityComparer<TResult>? comparer = null)
    {
        comparer ??= EqualityComparer<TResult>.Default;

        return HandleResult(r => comparer.Equals(r, result));
    }

    /// <summary>
    /// Builds the predicate.
    /// </summary>
    /// <returns>An instance of predicate delegate.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no predicates were configured using this builder.</exception>
    /// <remarks>
    /// The returned predicate will return <see langword="true"/> if any of the configured predicates return <see langword="true"/>.
    /// Please be aware of the performance penalty if you register too many predicates with this builder. In such case, it's better to create your own predicate
    /// manually as a delegate.
    /// </remarks>
    public Predicate<Outcome<TResult>> Build() => _predicates.Count switch
    {
        0 => throw new InvalidOperationException("No predicates were configured. There must be at least one predicate added."),
        1 => _predicates[0],
        _ => CreatePredicate(_predicates.ToArray()),
    };

    internal Func<TArgs, ValueTask<bool>> Build<TArgs>()
        where TArgs : IOutcomeArguments<TResult>
    {
        var predicate = Build();

        return args => new ValueTask<bool>(predicate(args.Outcome));
    }

    private static Predicate<Outcome<TResult>> CreatePredicate(Predicate<Outcome<TResult>>[] predicates)
        => outcome =>
           {
               foreach (var predicate in predicates)
               {
                   if (predicate(outcome))
                   {
                       return true;
                   }
               }

               return false;
           };

    private PredicateBuilder<TResult> Add(Predicate<Outcome<TResult>> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }
}
