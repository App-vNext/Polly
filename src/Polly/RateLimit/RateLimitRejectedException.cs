#nullable enable

namespace Polly.RateLimit;

/// <summary>
/// Exception thrown when a delegate executed through a <see cref="IRateLimitPolicy"/> is rate-limited.
/// </summary>
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
    public RateLimitRejectedException(TimeSpan retryAfter) : this(retryAfter, DefaultMessage(retryAfter))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, Exception innerException) : base(DefaultMessage(retryAfter), innerException) =>
        SetRetryAfter(retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="message">The message.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, string message) : base(message) =>
        SetRetryAfter(retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="retryAfter">The timespan after which the operation may be retried.</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitRejectedException(TimeSpan retryAfter, string message, Exception innerException) : base(message, innerException) =>
        SetRetryAfter(retryAfter);

    private void SetRetryAfter(TimeSpan retryAfter)
    {
        if (retryAfter < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(retryAfter), retryAfter, $"The {nameof(retryAfter)} parameter must be a TimeSpan greater than or equal to TimeSpan.Zero.");
        RetryAfter = retryAfter;
    }

    private static string DefaultMessage(TimeSpan retryAfter) =>
        $"The operation has been rate-limited and should be retried after {retryAfter}";
}
