using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Polly.Builder;

namespace Polly.RateLimiting;

/// <summary>
/// Options for the rate limiter strategy.
/// </summary>
public class RateLimiterStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterStrategyOptions"/> class.
    /// </summary>
    public RateLimiterStrategyOptions() => StrategyType = RateLimiterConstants.StrategyType;

    /// <summary>
    /// Gets or sets an event that is raised when the execution of user-provided callback is rejected by the rate limiter.
    /// </summary>
    [Required]
    public OnRateLimiterRejectedEvent OnRejected { get; set; } = new();

    /// <summary>
    ///  Gets or sets the rate limiter that the strategy uses.
    /// </summary>
    /// <remarks>
    /// This property is required and defaults to <c>null</c>.
    /// </remarks>
    [Required]
    public RateLimiter? RateLimiter { get; set; }
}
