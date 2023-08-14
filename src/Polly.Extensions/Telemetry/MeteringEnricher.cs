namespace Polly.Telemetry;

/// <summary>
/// Enricher used to enrich the metrics with additional information.
/// </summary>
public abstract class MeteringEnricher
{
    /// <summary>
    /// Enriches the metrics with additional information.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <typeparam name="TArgs">The type of arguments.</typeparam>
    /// <param name="context">The enrichment context.</param>
    public abstract void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context);
}
