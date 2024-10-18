#pragma warning disable CA1815 // Override equals and operator equals on value types

using System.Runtime.ExceptionServices;

namespace Polly;

/// <summary>
/// Represents the outcome of an operation which could be a result of type <typeparamref name="TResult"/> or an exception.
/// </summary>
/// <typeparam name="TResult">The result type of the operation.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct Outcome<TResult>
{
    internal Outcome(Exception exception)
        : this() => ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(Guard.NotNull(exception));

    internal Outcome(TResult? result)
        : this() => Result = result;

    internal Outcome(ExceptionDispatchInfo exceptionDispatchInfo)
        : this() => ExceptionDispatchInfo = Guard.NotNull(exceptionDispatchInfo);

    /// <summary>
    /// Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception => ExceptionDispatchInfo?.SourceException;

    /// <summary>
    /// Gets the <see cref="ExceptionDispatchInfo"/> associated with the exception, if any.
    /// </summary>
    internal ExceptionDispatchInfo? ExceptionDispatchInfo { get; }

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
    internal bool HasResult => ExceptionDispatchInfo is null;

    /// <summary>
    /// Gets a value indicating whether the operation produced a void result.
    /// </summary>
    internal bool IsVoidResult => Result is VoidResult;

    /// <summary>
    /// Throws an exception if the operation produced an exception.
    /// </summary>
    /// <remarks>
    /// If the operation produced a result, this method does nothing. The thrown exception maintains its original stack trace.
    /// </remarks>
    public void ThrowIfException() => ExceptionDispatchInfo?.Throw();

    /// <summary>
    /// Tries to get the result, if available.
    /// </summary>
    /// <param name="result">Output parameter for the result.</param>
    /// <returns><see langword="true"/> if the result is available; <see langword="false"/> otherwise.</returns>
    internal bool TryGetResult(out TResult? result)
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
    public override string ToString() => ExceptionDispatchInfo is not null
        ? Exception!.Message
        : Result?.ToString() ?? string.Empty;

    internal TResult GetResultOrRethrow()
    {
        ExceptionDispatchInfo?.Throw();
        return Result!;
    }
}
