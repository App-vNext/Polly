#if !NETCOREAPP
using System.Runtime.Serialization;
#endif

namespace Polly.Timeout;

/// <summary>
/// Exception thrown when a delegate executed through a timeout resilience strategy does not complete, before the configured timeout.
/// </summary>
#if !NETCOREAPP
[Serializable]
#endif
public class TimeoutRejectedException : ExecutionRejectedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException" /> class.
    /// </summary>
    public TimeoutRejectedException()
        : base("The operation didn't complete within the allowed timeout.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TimeoutRejectedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TimeoutRejectedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException" /> class.
    /// </summary>
    /// <param name="timeout">The timeout value that caused this exception.</param>
    public TimeoutRejectedException(TimeSpan timeout)
        : base("The operation didn't complete within the allowed timeout.") => Timeout = timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="timeout">The timeout value that caused this exception.</param>
    /// <param name="message">The message.</param>
    public TimeoutRejectedException(string message, TimeSpan timeout)
        : base(message) => Timeout = timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="timeout">The timeout value that caused this exception.</param>
    /// <param name="innerException">The inner exception.</param>
    public TimeoutRejectedException(string message, TimeSpan timeout, Exception innerException)
        : base(message, innerException) => Timeout = timeout;

    /// <summary>
    /// Gets the timeout value that caused this exception.
    /// </summary>
    public TimeSpan Timeout { get; private set; } = System.Threading.Timeout.InfiniteTimeSpan;

#pragma warning disable RS0016 // Add public types and members to the declared API
#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <param name="context">The context.</param>
    private TimeoutRejectedException(SerializationInfo info, StreamingContext context)
        : base(info, context) => Timeout = TimeSpan.FromSeconds(info.GetDouble("Timeout"));

    /// <inheritdoc/>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        Guard.NotNull(info);

        info.AddValue("Timeout", Timeout.TotalSeconds);

        base.GetObjectData(info, context);
    }
#endif
#pragma warning restore RS0016 // Add public types and members to the declared API
}
