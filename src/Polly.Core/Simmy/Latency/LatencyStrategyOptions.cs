using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Latency;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// Represents the options for the Latency chaos strategy.
/// </summary>
public class LatencyStrategyOptions : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    public sealed override string StrategyType => LatencyConstants.StrategyType;

    /// <summary>
    /// Gets or sets the delegate that's raised when delay occurs.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnDelayedArguments, ValueTask>? OnDelayed { get; set; }

    /// <summary>
    /// Gets or sets the latency generator that generates the delay for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. This property is required.
    /// </remarks>
    [Required]
    public Func<ResilienceContext, ValueTask<TimeSpan>> LatencyGenerator { get; set; }
}
