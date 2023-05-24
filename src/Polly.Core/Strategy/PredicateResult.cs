namespace Polly.Strategy;

/// <summary>
/// Class that represents the results that can be used in predicates.
/// </summary>
public static class PredicateResult
{
    /// <summary>
    /// Gets a finished <see cref="ValueTask{TResult}"/> that returns <see langword="true"/> value.
    /// </summary>
    public static ValueTask<bool> True => new(true);

    /// <summary>
    /// Gets a finished <see cref="ValueTask{TResult}"/> that returns <see langword="false"/> value.
    /// </summary>
    public static ValueTask<bool> False => new(false);
}
