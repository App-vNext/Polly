using System.ComponentModel.DataAnnotations;
using Polly.Telemetry;

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

    /// <summary>
    /// Gets or sets an instance of <see cref="TelemetryFactory"/>.
    /// </summary>
    [Required]
    public ResilienceTelemetryFactory TelemetryFactory { get; set; } = NullResilienceTelemetryFactory.Instance;

    /// <summary>
    /// Gets or sets a <see cref="TimeProvider"/> that is used by strategies that work with time.
    /// </summary>
    /// <remarks>
    /// This property is internal until we switch to official System.TimeProvider.
    /// </remarks>
    [Required]
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
