namespace Polly;

/// <summary>
/// The backoff type used by the strategies.
/// </summary>
public enum DelayBackoffType
{
    /// <summary>
    /// The constant backoff type.
    /// </summary>
    /// <example>
    /// 200ms, 200ms, 200ms, etc.
    /// </example>
    /// <remarks>
    /// Ensures a constant backoff for each attempt.
    /// </remarks>
    Constant,

    /// <summary>
    /// The linear backoff type.
    /// </summary>
    /// <example>
    /// 100ms, 200ms, 300ms, 400ms, etc.
    /// </example>
    /// <remarks>
    /// Generates backoffs in an linear manner.
    /// In the case randomization introduced by the jitter and exponential growth are not appropriate,
    /// the linear growth allows for more precise control over the backoff intervals.
    /// </remarks>
    Linear,

    /// <summary>
    /// The exponential backoff type with the power of 2.
    /// </summary>
    /// <example>
    /// 200ms, 400ms, 800ms.
    /// </example>
    Exponential,
}
