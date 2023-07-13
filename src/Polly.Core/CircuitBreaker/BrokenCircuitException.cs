#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

namespace Polly.CircuitBreaker;

/// <summary>
/// Exception thrown when a circuit is broken.
/// </summary>
#if !NETCOREAPP
[Serializable]
#endif
public class BrokenCircuitException : ExecutionRejectedException
{
    internal const string DefaultMessage = "The circuit is now open and is not allowing calls.";

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
    /// </summary>
    public BrokenCircuitException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BrokenCircuitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The inner exception.</param>
    public BrokenCircuitException(string message, Exception inner)
        : base(message, inner)
    {
    }

#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected private BrokenCircuitException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
