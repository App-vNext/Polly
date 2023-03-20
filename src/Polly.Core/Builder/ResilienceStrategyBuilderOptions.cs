using System.ComponentModel.DataAnnotations;

namespace Polly.Builder;

/// <summary>
/// The builder options used by <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public class ResilienceStrategyBuilderOptions
{
    /// <summary>
    /// Gets or sets the name of the builder.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.</remarks>
    [Required(AllowEmptyStrings = true)]
    public string BuilderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the custom properties attached to builder options.
    /// </summary>
    public ResilienceProperties Properties { get; } = new();
}
