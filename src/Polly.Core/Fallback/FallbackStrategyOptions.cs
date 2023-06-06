using System.ComponentModel.DataAnnotations;

namespace Polly.Fallback;

/// <summary>
/// Represents the options for configuring a fallback resilience strategy.
/// </summary>
internal class FallbackStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>Fallback</c> value.</remarks>
    public sealed override string StrategyType => FallbackConstants.StrategyType;

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
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OutcomeArguments<object, OnFallbackArguments>, ValueTask>? OnFallback { get; set; }
}

