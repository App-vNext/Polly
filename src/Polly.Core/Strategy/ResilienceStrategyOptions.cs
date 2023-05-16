using System.ComponentModel.DataAnnotations;

namespace Polly.Strategy;

/// <summary>
/// The options associated with the <see cref="ResilienceStrategy"/>.
/// </summary>
public class ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the name of the strategy.
    /// </summary>
    /// <remarks>
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// Defaults to <see cref="string.Empty"/>.
    /// </remarks>
    [Required(AllowEmptyStrings = true)]
    public string StrategyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the strategy.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.</remarks>
    /// Defaults to <see cref="string.Empty"/>.
    [Required(AllowEmptyStrings = true)]
    public string StrategyType { get; set; } = string.Empty;
}
