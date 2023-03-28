using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Polly.Telemetry;

namespace Polly.Builder;

/// <summary>
/// The builder options used by <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public class ResilienceStrategyBuilderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyBuilderOptions"/> class.
    /// </summary>
    public ResilienceStrategyBuilderOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyBuilderOptions"/> class.
    /// </summary>
    /// <param name="other">The options to copy the values from.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ResilienceStrategyBuilderOptions(ResilienceStrategyBuilderOptions other)
    {
        Guard.NotNull(other);

        BuilderName = other.BuilderName;
        TelemetryFactory = other.TelemetryFactory;
        TimeProvider = other.TimeProvider;

        IDictionary<string, object?> props = Properties;

        foreach (KeyValuePair<string, object?> pair in other.Properties)
        {
            props.Add(pair.Key, pair.Value);
        }
    }

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
