namespace Polly.Retry;

/// <summary>
/// The backoff type used by the retry strategy.
/// </summary>
public enum RetryBackoffType
{
    /// <summary>
    /// Exponential delay with randomization retry type,
    /// making sure to mitigate any correlations.
    /// </summary>
    /// <example>
    /// 850ms, 1455ms, 3060ms.
    /// </example>
    /// <remarks>
    /// In transient failures handling scenarios, this is the
    /// <see href=" https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation"> recommended retry type</see>.
    /// </remarks>
    ExponentialWithJitter,

    /// <summary>
    /// The constant retry type.
    /// </summary>
    /// <example>
    /// 200ms, 200ms, 200ms, etc.
    /// </example>
    /// <remarks>
    /// Ensures a constant wait duration before each retry attempt.
    /// For concurrent database access with a possibility of conflicting updates,
    /// retrying the failures in a constant manner allows for consistent transient failure mitigation.
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
    /// In the case randomization introduced by the jitter and exponential growth are not appropriate,
    /// the linear growth allows for more precise control over the delay intervals.
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
