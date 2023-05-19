using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Polly.Strategy;

namespace Polly.RateLimiting;

/// <summary>
/// Options for the rate limiter strategy.
/// </summary>
public class RateLimiterStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>RateLimiter</c> value.</remarks>
    public sealed override string StrategyType => RateLimiterConstants.StrategyType;

    /// <summary>
    /// Gets or sets an event that is raised when the execution of user-provided callback is rejected by the rate limiter.
    /// </summary>
    [Required]
    public NoOutcomeEvent<OnRateLimiterRejectedArguments> OnRejected { get; set; } = new();

    /// <summary>
    ///  Gets or sets the rate limiter that the strategy uses.
    /// </summary>
    /// <remarks>
    /// This property is required and defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public RateLimiter? RateLimiter { get; set; }
}
