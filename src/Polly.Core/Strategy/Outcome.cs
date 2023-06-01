#pragma warning disable CA1815 // Override equals and operator equals on value types

using System.Runtime.ExceptionServices;

namespace Polly.Strategy;

/// <summary>
/// Represents the outcome of an operation which could be a result of type <typeparamref name="TResult"/> or an exception.
/// </summary>
/// <typeparam name="TResult">The result type of the operation.</typeparam>
public readonly struct Outcome<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> struct.
    /// </summary>
    /// <param name="exception">The occurred exception during the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
    public Outcome(Exception exception)
        : this() => ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(Guard.NotNull(exception));

    internal Outcome(ExceptionDispatchInfo exceptionDispatchInfo)
        : this() => ExceptionDispatchInfo = Guard.NotNull(exceptionDispatchInfo);

    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> struct.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public Outcome(TResult? result)
        : this() => Result = result;

    /// <summary>
    /// Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception => ExceptionDispatchInfo?.SourceException;

    /// <summary>
    /// Gets the <see cref="ExceptionDispatchInfo"/> associated with the exception, if any.
    /// </summary>
    internal ExceptionDispatchInfo? ExceptionDispatchInfo { get; }

    internal ValueTask<Outcome<TResult>> AsValueTask() => new(this);

    /// <summary>
    /// Gets the result of the operation, if any.
    /// </summary>
    public TResult? Result { get; }

    /// <summary>
    /// Gets a value indicating whether the operation produced a result.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> even if the result is void. Use <see cref="IsVoidResult"/> to check for void results.
    /// </remarks>
    public bool HasResult => ExceptionDispatchInfo == null;

    /// <summary>
    /// Gets a value indicating whether the operation produced a void result.
    /// </summary>
    public bool IsVoidResult => Result is VoidResult;

    /// <summary>
    /// Throws an exception if the operation produced an exception.
    /// </summary>
    /// <remarks>
    /// If the operation produced a result, this method does nothing. The thrown exception maintains its original stack trace.
    /// </remarks>
    public void EnsureSuccess() => ExceptionDispatchInfo?.Throw();

    /// <summary>
    /// Tries to get the result, if available.
    /// </summary>
    /// <param name="result">Output parameter for the result.</param>
    /// <returns><see langword="true"/> if the result is available; <see langword="false"/> otherwise.</returns>
    public bool TryGetResult(out TResult? result)
    {
        if (HasResult && !IsVoidResult)
        {
            result = Result!;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Returns the string representation of the outcome.
    /// </summary>
    /// <returns>
    /// The exception message if the outcome is an exception; otherwise, the string representation of the result.
    /// </returns>
    public override string ToString() => ExceptionDispatchInfo != null
        ? Exception!.Message
        : Result?.ToString() ?? string.Empty;

    internal TResult GetResultOrRethrow()
    {
        ExceptionDispatchInfo?.Throw();
        return Result!;
    }

    internal Outcome<object> AsOutcome() => AsOutcome<object>();

    internal Outcome<T> AsOutcome<T>() => (ExceptionDispatchInfo != null)
        ? new Outcome<T>(ExceptionDispatchInfo)
        : new Outcome<T>((T)(object)Result!);
}
