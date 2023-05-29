using System.Collections.Generic;
using Polly.Strategy;

namespace Polly.Extensions.Telemetry;

/// <summary>
/// Enrichment context used when reporting resilience telemetry. This context is passed to the registered enrichers in <see cref="TelemetryResilienceStrategyOptions.Enrichers"/>.
/// </summary>
public sealed partial class EnrichmentContext
{
    private EnrichmentContext()
    {
    }

    /// <summary>
    /// Gets the outcome of the operation if any.
    /// </summary>
    public Outcome<object>? Outcome { get; internal set; }

    /// <summary>
    /// Gets the resilience arguments associated with the resilience event, if any.
    /// </summary>
    public IResilienceArguments? ResilienceArguments { get; internal set; }

    /// <summary>
    /// Gets the resilience context associated with the operation that produced the resilience event.
    /// </summary>
    public ResilienceContext ResilienceContext { get; internal set; } = null!;

    /// <summary>
    /// Gets the tags associated with the resilience event.
    /// </summary>
    public ICollection<KeyValuePair<string, object?>> Tags { get; } = new List<KeyValuePair<string, object?>>();
}
