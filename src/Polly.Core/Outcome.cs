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
    public static ValueTask<Outcome<TResult>> FromResultAsValueTask<TResult>(TResult value) => new(FromResult(value));

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
    public static ValueTask<Outcome<TResult>> FromExceptionAsValueTask<TResult>(Exception exception)
    {
        Guard.NotNull(exception);

        return new(FromException<TResult>(exception));
    }

    internal static ValueTask<Outcome<TResult>> FromValueTask<TResult>(ValueTask<TResult> valueTask, bool continueOnCapturedContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return FromResultAsValueTask(valueTask.Result);
        }

        return FromValueTaskAsync(valueTask, continueOnCapturedContext);
    }

    internal static ValueTask<Outcome<VoidResult>> FromValueTask(ValueTask valueTask, bool continueOnCapturedContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<VoidResult>>(Void);
        }

        return FromValueTaskAsync(valueTask, continueOnCapturedContext);
    }

    internal static Outcome<VoidResult> Void => FromResult(VoidResult.Instance);

    internal static Outcome<VoidResult> FromException(Exception exception) => FromException<VoidResult>(exception);

    private static async ValueTask<Outcome<TResult>> FromValueTaskAsync<TResult>(ValueTask<TResult> valueTask, bool continueOnCapturedContext)
    {
        try
        {
            return FromResult(await valueTask.ConfigureAwait(continueOnCapturedContext));
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            return FromException<TResult>(e);
        }
    }

    private static async ValueTask<Outcome<VoidResult>> FromValueTaskAsync(ValueTask valueTask, bool continueOnCapturedContext)
    {
        try
        {
            await valueTask.ConfigureAwait(continueOnCapturedContext);
            return Void;
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            return FromException(e);
        }
    }
}
