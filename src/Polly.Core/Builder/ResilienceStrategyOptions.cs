using System.ComponentModel.DataAnnotations;

namespace Polly.Builder;

/// <summary>
/// The options associated with the <see cref="ResilienceStrategy"/>.
/// </summary>
public class ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the name of the strategy.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.</remarks>
    [Required(AllowEmptyStrings = true)]
    public string StrategyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the strategy.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.</remarks>
    [Required(AllowEmptyStrings = true)]
    public string StrategyType { get; set; } = string.Empty;
}
