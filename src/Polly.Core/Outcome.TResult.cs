#pragma warning disable CA1815 // Override equals and operator equals on value types

using System;
using System.Runtime.CompilerServices;
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

    private Outcome(ExceptionDispatchInfo exceptionDispatchInfo)
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
    public bool HasResult => ExceptionDispatchInfo is null;

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
    public override string ToString() => ExceptionDispatchInfo is not null
        ? Exception!.Message
        : Result?.ToString() ?? string.Empty;

    internal TResult GetResultOrRethrow()
    {
        ExceptionDispatchInfo?.Throw();
        return Result!;
    }

    internal Outcome<object> AsOutcome() => AsOutcome<object>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Outcome<T> AsOutcome<T>()
    {
        if (ExceptionDispatchInfo is not null)
        {
            return new Outcome<T>(ExceptionDispatchInfo);
        }

        if (Result is null)
        {
            return new Outcome<T>(default(T));
        }

        if (typeof(T) == typeof(TResult))
        {
            var result = Result;

            // We can use the unsafe cast here because we know for sure these two types are the same
            return new Outcome<T>(Unsafe.As<TResult, T>(ref result));
        }

        return new Outcome<T>((T)(object)Result);
    }
}
