#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Polly.Strategy;

/// <summary>
/// Represents the non-generic outcome of an operation.
/// </summary>
public readonly struct Outcome
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome"/> struct with the specified exception.
    /// </summary>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public Outcome(Exception exception)
        : this() => Exception = Guard.NotNull(exception);

    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome"/> struct with the specified result.
    /// </summary>
    /// <param name="result">The result produced by the operation.</param>
    public Outcome(object? result)
        : this() => Result = result;

    /// <summary>
    /// The object de-constructor.
    /// </summary>
    /// <param name="result">The outcome result, if any.</param>
    /// <param name="exception">The outcome exception, if any.</param>
    public void Deconstruct(out object? result, out Exception? exception)
    {
        exception = Exception;
        result = Result;
    }

    /// <summary>
    /// Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the result produced by the operation, if any.
    /// </summary>
    public object? Result { get; }

    /// <summary>
    /// Gets a value indicating whether the operation produced a result.
    /// </summary>
    /// <remarks>
    /// If the operation returned a void result the value will be <see langword="true"/>.
    /// You can use <see cref="IsVoidResult"/> to determine if the result is a void result.
    /// </remarks>
    public bool HasResult => Exception == null;

    /// <summary>
    /// Gets a value indicating whether the operation produced a void result.
    /// </summary>
    public bool IsVoidResult => Result is VoidResult;

    /// <summary>
    /// Tries to get a result if available.
    /// </summary>
    /// <param name="result">The result instance.</param>
    /// <returns>True if result is available, false otherwise.</returns>
    public bool TryGetResult(out object? result)
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
    /// Gets the string representation of the outcome.
    /// </summary>
    /// <returns>A string representation of the outcome.</returns>
    /// <remarks>
    /// If the outcome represents an exception, then <see cref="Exception.Message"/> will be returned.
    /// If the outcome represents a result, then <see cref="Result"/> formatted as string will be returned.
    /// </remarks>
    public override string ToString()
    {
        if (Exception != null)
        {
            return Exception.Message;
        }

        return Result?.ToString() ?? string.Empty;
    }

    internal Outcome<TResult> AsOutcome<TResult>()
    {
        if (Exception != null)
        {
            return new Outcome<TResult>(Exception);
        }

        return new Outcome<TResult>((TResult)Result!);
    }
}
