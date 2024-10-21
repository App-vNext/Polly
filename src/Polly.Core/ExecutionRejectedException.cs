#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

using Polly.Telemetry;

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

#pragma warning disable RS0016 // Add public types and members to the declared API
#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected ExecutionRejectedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
#pragma warning restore RS0016 // Add public types and members to the declared API

    /// <summary>
    /// Gets the source of the strategy which has thrown the exception, if known.
    /// </summary>
    public virtual ResilienceTelemetrySource? TelemetrySource { get; internal set; }

}
