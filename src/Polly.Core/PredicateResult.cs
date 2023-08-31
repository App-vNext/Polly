namespace Polly;

/// <summary>
/// Class that represents the results that can be used in predicates.
/// </summary>
public static class PredicateResult
{
    /// <summary>
    /// Returns a finished <see cref="ValueTask{TResult}"/> that returns <see langword="true"/> value.
    /// </summary>
    /// <returns>A new instance of finished <see cref="ValueTask{TResult}"/>.</returns>
    public static ValueTask<bool> True() => new(true);

    /// <summary>
    /// Returns a finished <see cref="ValueTask{TResult}"/> that returns <see langword="false"/> value.
    /// </summary>
    /// <returns>A new instance of finished <see cref="ValueTask{TResult}"/>.</returns>
    public static ValueTask<bool> False() => new(false);
}
