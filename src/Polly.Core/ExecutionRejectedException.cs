#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

namespace Polly;

/// <summary>
/// Exception thrown when a policy rejects execution of a delegate.
/// <remarks>More specific exceptions which derive from this type, are generally thrown.</remarks>
/// </summary>
public abstract class ExecutionRejectedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
    /// </summary>
    protected ExecutionRejectedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected ExecutionRejectedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The inner exception.</param>
    protected ExecutionRejectedException(string message, Exception inner)
        : base(message, inner)
    {
    }

#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
#pragma warning disable RS0016 // Add public types and members to the declared API
    protected ExecutionRejectedException(SerializationInfo info, StreamingContext context)
#pragma warning restore RS0016 // Add public types and members to the declared API
        : base(info, context)
    {
    }
#endif
}
