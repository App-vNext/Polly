namespace Polly;

/// <summary>
/// Produces instances of <see cref="Outcome{TResult}"/>.
/// </summary>
public static class Outcome
{
    /// <summary>
    /// Returns a <see cref="Outcome{TResult}"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>An instance of <see cref="Outcome{TResult}"/>.</returns>
    public static Outcome<TResult> FromResult<TResult>(TResult? value) => new(value);

    /// <summary>
    /// Returns a <see cref="Outcome{TResult}"/> with the given <paramref name="value"/> wrapped as <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>A completed <see cref="ValueTask{TResult}"/> that produces <see cref="Outcome{TResult}"/>.</returns>
    public static ValueTask<Outcome<TResult>> FromResultAsTask<TResult>(TResult value) => new(FromResult(value));

    /// <summary>
    /// Returns a <see cref="Outcome{TResult}"/> with the given <paramref name="exception"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="exception">The exception.</param>
    /// <returns>An instance of <see cref="Outcome{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static Outcome<TResult> FromException<TResult>(Exception exception)
    {
        Guard.NotNull(exception);

        return new(exception);
    }

    /// <summary>
    /// Returns a <see cref="Outcome{TResult}"/> with the given <paramref name="exception"/> wrapped as <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="exception">The exception.</param>
    /// <returns>A completed <see cref="ValueTask{TResult}"/> that produces <see cref="Outcome{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static ValueTask<Outcome<TResult>> FromExceptionAsTask<TResult>(Exception exception)
    {
        Guard.NotNull(exception);

        return new(FromException<TResult>(exception));
    }

    internal static Outcome<VoidResult> Void => FromResult(VoidResult.Instance);

    internal static Outcome<VoidResult> FromException(Exception exception) => FromException<VoidResult>(exception);

    internal static Outcome<object> ToObjectOutcome<T>(Outcome<T> outcome)
    {
        if (outcome.ExceptionDispatchInfo is null)
        {
            return FromResult((object?)outcome.Result);
        }

        return new Outcome<object>(outcome.ExceptionDispatchInfo);
    }

    internal static Outcome<T> FromObjectOutcome<T>(Outcome<object> outcome)
    {
        if (outcome.ExceptionDispatchInfo is null)
        {
            return FromResult((T)outcome.Result!);
        }

        return new Outcome<T>(outcome.ExceptionDispatchInfo);
    }
}
