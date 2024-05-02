namespace Polly;

/// <summary>
/// The captured outcome of executing an individual Func&lt;TResult&gt;.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class DelegateResult<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateResult{TResult}"/> class representing an execution which returned <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public DelegateResult(TResult result) => Result = result;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateResult{TResult}"/> class representing an execution which threw <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The exception.</param>
    public DelegateResult(Exception exception) =>
        Exception = exception;

    /// <summary>
    /// Gets the result of executing the delegate. Will be default(TResult) if an exception was thrown.
    /// </summary>
    public TResult Result { get; }

    /// <summary>
    /// Gets the exception thrown while executing the delegate. Will be null if policy executed without exception.
    /// </summary>
    public Exception Exception { get; }
}
