#nullable enable

#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Polly.RateLimit;

#pragma warning disable RS0016 // Add public types and members to the declared API

/// <summary>
/// Exception thrown when a delegate executed through a <see cref="IRateLimitPolicy"/> is rate-limited.
/// </summary>
#if NETSTANDARD2_0
[Serializable]
#endif
public class RateLimitRejectedException : ExecutionRejectedException
{
    /// <summary>
    /// The timespan after which the operation may be retried.
    /// </summary>
    public TimeSpan RetryAfter { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    public RateLimitRejectedException(TimeSpan retryAfter)
        : this(retryAfter, DefaultMessage(retryAfter))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, Exception innerException)
        : base(DefaultMessage(retryAfter), innerException) => SetRetryAfter(retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="message">The message.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, string message)
        : base(message) => SetRetryAfter(retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, string message, Exception innerException)
        : base(message, innerException) => SetRetryAfter(retryAfter);

    private static string DefaultMessage(TimeSpan retryAfter) =>
        $"The operation has been rate-limited and should be retried after {retryAfter}";

    private void SetRetryAfter(TimeSpan retryAfter)
    {
        if (retryAfter < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retryAfter), retryAfter, $"The {nameof(retryAfter)} parameter must be a TimeSpan greater than or equal to TimeSpan.Zero.");
        RetryAfter = retryAfter;
    }

#if NETSTANDARD2_0
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <param name="context">The context.</param>
    protected RateLimitRejectedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
