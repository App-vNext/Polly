using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents the options for configuring a fallback resilience strategy.
/// </summary>
public class FallbackStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FallbackStrategyOptions"/> class.
    /// </summary>
    public FallbackStrategyOptions() => StrategyType = FallbackConstants.StrategyType;

    /// <summary>
    /// Gets or sets the <see cref="FallbackHandler"/> instance used for configure fallback scenarios.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    [Required]
    public FallbackHandler Handler { get; set; } = new();

    /// <summary>
    /// Gets or sets the outcome event instance for raising fallback events.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    [Required]
    public OutcomeEvent<OnFallbackArguments> OnFallback { get; set; } = new();
}

