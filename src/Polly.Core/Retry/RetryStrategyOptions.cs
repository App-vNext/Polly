using System.ComponentModel.DataAnnotations;

namespace Polly.Retry;

/// <summary>
/// Represents the options used to configure a retry strategy.
/// </summary>
public class RetryStrategyOptions
{
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
}
