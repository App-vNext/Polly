using System.ComponentModel.DataAnnotations;
using Polly.Builder;

namespace Polly.Retry;

/// <summary>
/// Represents the options used to configure a retry strategy.
/// </summary>
public class RetryStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryStrategyOptions"/> class.
    /// </summary>
    public RetryStrategyOptions() => StrategyType = RetryConstants.StrategyType;

    /// <summary>
    /// Value that represents infinite retries.
    /// </summary>
    public const int InfiniteRetryCount = RetryConstants.InfiniteRetryCount;

    /// <summary>
    /// Gets or sets the maximum number of retries to use, in addition to the original call.
    /// </summary>
    /// <remarks>
    /// Defaults to 3 retries. For infinite retries use <c>InfiniteRetry</c> (-1).
    /// </remarks>
    [Range(InfiniteRetryCount, RetryConstants.MaxRetryCount)]
    public int RetryCount { get; set; } = RetryConstants.DefaultRetryCount;

    /// <summary>
    /// Gets or sets the type of the back-off.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="RetryBackoffType.Exponential"/>.
    /// </remarks>
    public RetryBackoffType BackoffType { get; set; } = RetryConstants.DefaultBackoffType;

    /// <summary>
    /// Gets or sets the delay between retries based on the backoff type, <see cref="RetryBackoffType"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to 2 seconds.
    /// For <see cref="RetryBackoffType.Exponential"/> this represents the median delay to target before the first retry.
    /// For the <see cref="RetryBackoffType.Linear"/> it represents the initial delay, the following delays increasing linearly with this value.
    /// In case of <see cref="RetryBackoffType.Constant"/> it represents the constant delay between retries.
    /// </remarks>
    [TimeSpan("00:00:00", "1.00:00:00")]
    public TimeSpan BaseDelay { get; set; } = RetryConstants.DefaultBaseDelay;

    /// <summary>
    /// Gets or sets the <see cref="ShouldRetryPredicate"/> instance used to determine if a retry should be performed.
    /// </summary>
    /// <remarks>
    /// By default, the predicate is empty and no results or exceptions are retried.
    /// </remarks>
    [Required]
    public ShouldRetryPredicate ShouldRetry { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="RetryDelayGenerator"/> instance that is used to generated the delay between retries.
    /// </summary>
    /// <remarks>
    /// By default, the generator is empty and it does not affect the delay between retries.
    /// </remarks>
    [Required]
    public RetryDelayGenerator RetryDelayGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="OnRetryEvent"/> instance that is invoked when retry happens.
    /// </summary>
    /// <remarks>
    /// By default, the event is empty and no callbacks are registered.
    /// </remarks>
    [Required]
    public OnRetryEvent OnRetry { get; set; } = new();
}
