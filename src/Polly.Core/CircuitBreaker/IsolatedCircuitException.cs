#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

namespace Polly.CircuitBreaker;

/// <summary>
/// Exception thrown when a circuit is isolated (held open) by manual override.
/// </summary>
#if !NETCOREAPP
[Serializable]
#endif
public class IsolatedCircuitException : BrokenCircuitException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
    /// </summary>
    public IsolatedCircuitException()
        : base("The circuit is manually held open and is not allowing calls.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public IsolatedCircuitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public IsolatedCircuitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#pragma warning disable RS0016 // Add public types and members to the declared API
#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected IsolatedCircuitException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
#pragma warning restore RS0016 // Add public types and members to the declared API
}
