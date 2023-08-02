namespace Polly.Extensions.Telemetry;

/// <summary>
/// Enrichment context used when reporting resilience telemetry. This context is passed to the registered enrichers in <see cref="TelemetryOptions.Enrichers"/>.
/// </summary>
public sealed partial class EnrichmentContext
{
    private const int InitialArraySize = 20;

    private readonly KeyValuePair<string, object?>[] _tagsArray = new KeyValuePair<string, object?>[InitialArraySize];

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
    public object? Arguments { get; internal set; }

    /// <summary>
    /// Gets the resilience context associated with the operation that produced the resilience event.
    /// </summary>
    public ResilienceContext Context { get; internal set; } = null!;

    /// <summary>
    /// Gets the tags associated with the resilience event.
    /// </summary>
    public IList<KeyValuePair<string, object?>> Tags { get; } = new List<KeyValuePair<string, object?>>();

    internal ReadOnlySpan<KeyValuePair<string, object?>> TagsSpan
    {
        get
        {
            // stryker disable once equality : no means to test this
            if (Tags.Count > _tagsArray.Length)
            {
                Array.Resize(ref _tagsArray, Tags.Count);
            }

            for (int i = 0; i < Tags.Count; i++)
            {
                _tagsArray[i] = Tags[i];
            }

            return _tagsArray.AsSpan(0, Tags.Count);
        }
    }
}
