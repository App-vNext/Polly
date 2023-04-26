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
    /// <param name="resultType">The type of the result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public Outcome(Type resultType, Exception exception)
        : this()
    {
        ResultType = Guard.NotNull(resultType);
        Exception = Guard.NotNull(exception);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome"/> struct with the specified result.
    /// </summary>
    /// <param name="resultType">The type of the result.</param>
    /// <param name="result">The result produced by the operation.</param>
    public Outcome(Type resultType, object? result)
        : this()
    {
        ResultType = Guard.NotNull(resultType);
        Result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the outcome is valid.
    /// </summary>
    public bool IsValid => ResultType != null;

    /// <summary>
    /// Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the result produced by the operation, if any.
    /// </summary>
    public object? Result { get; }

    /// <summary>
    /// Gets the result type produced by the operation.
    /// </summary>
    public Type ResultType { get; }

    /// <summary>
    /// Gets a value indicating whether the operation produced a result.
    /// </summary>
    /// <remarks>
    /// If the operation returned a void result the value will be <c>true</c>.
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

    /// <inheritdoc/>
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
