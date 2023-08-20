namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Enrichment context used when reporting resilience events.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TArgs">The type of the arguments attached to the resilience event.</typeparam>
/// <remarks>
/// This context is passed to enrichers in <see cref="TelemetryOptions.MeteringEnrichers"/>.
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct EnrichmentContext<TResult, TArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnrichmentContext{TResult, TArgs}"/> struct.
    /// </summary>
    /// <param name="telemetryEvent">The telemetry event info.</param>
    /// <param name="tags">Tags associated with the resilience event.</param>
    public EnrichmentContext(in TelemetryEventArguments<TResult, TArgs> telemetryEvent, IList<KeyValuePair<string, object?>> tags)
    {
        TelemetryEvent = telemetryEvent;
        Tags = tags;
    }

    /// <summary>
    /// Gets the info about the telemetry event.
    /// </summary>
    public TelemetryEventArguments<TResult, TArgs> TelemetryEvent { get; }

    /// <summary>
    /// Gets the tags associated with the resilience event.
    /// </summary>
    /// <remarks>
    /// Custom enricher can add tags to this collection.
    /// </remarks>
    public IList<KeyValuePair<string, object?>> Tags { get; }
}
