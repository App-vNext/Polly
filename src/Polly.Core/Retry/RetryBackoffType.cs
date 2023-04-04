namespace Polly.Retry;

/// <summary>
/// The backoff type used by the retry strategy.
/// </summary>
public enum RetryBackoffType
{
    /// <summary>
    /// The constant retry type.
    /// </summary>
    /// <example>
    /// 200ms, 200ms, 200ms, etc.
    /// </example>
    /// <remarks>
    /// It ensures a constant wait duration before each retry attempt.
    /// For concurrent database access with possibility of conflicting updates,
    /// retrying the failures in a constant manner allows consistent transient failures mitigation.
    /// </remarks>
    Constant,

    /// <summary>
    /// The linear retry type.
    /// </summary>
    /// <example>
    /// 100ms, 200ms, 300ms, 400ms, etc.
    /// </example>
    /// <remarks>
    /// Generates sleep durations in an linear manner.
    /// In case randomization introduced by the jitter and exponential growth are not intended,
    /// the linear growth allows more control over the delay intervals.
    /// </remarks>
    Linear,

    /// <summary>
    /// The exponential delay type with the power of 2.
    /// </summary>
    /// <example>
    /// 200ms, 400ms, 800ms.
    /// </example>
    Exponential,
}
