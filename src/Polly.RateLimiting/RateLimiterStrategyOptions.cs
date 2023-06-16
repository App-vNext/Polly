using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

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
    /// Gets or sets the default rate limiter options.
    /// </summary>
    /// <remarks>
    /// The options for the default limiter that will be used when <see cref="RateLimiter"/> is <see langword="null"/>.
    /// <para>
    /// <see cref="ConcurrencyLimiterOptions.PermitLimit"/> defaults to 1000.
    /// <see cref="ConcurrencyLimiterOptions.QueueLimit"/> defaults to 0.
    /// </para>
    /// </remarks>
    [Required]
    public ConcurrencyLimiterOptions DefaultRateLimiterOptions { get; set; } = new()
    {
        QueueLimit = RateLimiterConstants.DefaultQueueLimit,
        PermitLimit = RateLimiterConstants.DefaultPermitLimit,
    };

    /// <summary>
    /// Gets or sets an event that is raised when the execution of user-provided callback is rejected by the rate limiter.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnRejected { get; set; }

    /// <summary>
    ///  Gets or sets the rate limiter that the strategy uses.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. If this property is <see langword="null"/>,
    /// then the strategy will use a <see cref="ConcurrencyLimiter"/> created using <see cref="DefaultRateLimiterOptions"/>.
    /// </remarks>
    public RateLimiter? RateLimiter { get; set; }
}
