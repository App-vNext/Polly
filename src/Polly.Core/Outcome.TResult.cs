#pragma warning disable CA1815 // Override equals and operator equals on value types

using System.ComponentModel;
#if UNION_TYPES
using System.Runtime.CompilerServices;
#endif
using System.Runtime.ExceptionServices;

namespace Polly;

/// <summary>
/// Represents the outcome of an operation which could be a result of type <typeparamref name="TResult"/> or an exception.
/// </summary>
/// <typeparam name="TResult">The result type of the operation.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
#if UNION_TYPES
[Union]
public readonly struct Outcome<TResult> : IUnion
#else
public readonly struct Outcome<TResult>
#endif
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> struct.
    /// </summary>
    /// <param name="exception">The exception that occurred during the operation.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Outcome(Exception exception)
        : this() => ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(Guard.NotNull(exception));

    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> struct.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Outcome(TResult? result)
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

#if UNION_TYPES
    /// <summary>
    /// Gets a value indicating whether the result or an exception is present.
    /// </summary>
    /// <remarks>
    /// This property is used to support C# union compiler infrastructure and should not be used directly by user code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool HasValue => HasResult || Exception is not null;

    /// <inheritdoc/>
    public object? Value => HasResult switch
    {
        true => Result,
        false => Exception,
    };

    /// <summary>
    /// Tries to get the exception associated with the outcome, if any.
    /// </summary>
    /// <param name="value">
    /// The exception that occurred during the operation, if any; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <see cref="Exception"/> is not <see langword="null"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is used to support C# union compiler infrastructure and should not be used directly by user code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool TryGetValue(out Exception? value)
    {
        value = Exception;
        return value is not null;
    }

    /// <summary>
    /// Tries to get the result associated with the outcome, if any.
    /// </summary>
    /// <param name="value">
    /// The result of the operation, if any; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="false"/> if <see cref="Exception"/> is not <see langword="null"/>;
    /// otherwise <see langword="true"/>.
    /// </returns>
    /// <remarks>
    /// This method is used to support C# union compiler infrastructure and should not be used directly by user code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool TryGetValue(out TResult? value)
    {
        value = default;

        if (!HasResult)
        {
            return false;
        }

        value = Result;
        return true;
    }
#endif

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
