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
    [Required]
    public ShouldRetryPredicate ShouldRetry { get; set; } = new();
}
