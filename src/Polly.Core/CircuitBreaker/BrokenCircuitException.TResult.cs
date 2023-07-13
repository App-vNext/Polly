#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

namespace Polly.CircuitBreaker;

#pragma warning disable CA1032 // Implement standard exception constructors

/// <summary>
/// Exception thrown when a circuit is broken.
/// </summary>
/// <typeparam name="TResult">The type of returned results being handled by the policy.</typeparam>
#if !NETCOREAPP
[Serializable]
#endif
public class BrokenCircuitException<TResult> : BrokenCircuitException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
    /// </summary>
    /// <param name="result">The result which caused the circuit to break.</param>
    public BrokenCircuitException(TResult result) => Result = result;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="result">The result which caused the circuit to break.</param>
    public BrokenCircuitException(string message, TResult result)
        : base(message) => Result = result;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The inner exception.</param>
    /// <param name="result">The result which caused the circuit to break.</param>
    public BrokenCircuitException(string message, Exception inner, TResult result)
        : base(message, inner) => Result = result;

    /// <summary>
    /// Gets the result value which was considered a handled fault, by the policy.
    /// </summary>
    public TResult Result { get; }

#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    private BrokenCircuitException(SerializationInfo info, StreamingContext context)
        : base(info, context) => Result = default!;
#endif
}
