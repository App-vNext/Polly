namespace Polly.Telemetry;

internal static class EnrichmentUtil
{
    public static void Enrich(EnrichmentContext context, List<Action<EnrichmentContext>> enrichers)
    {
        if (enrichers.Count == 0)
        {
            return;
        }

        foreach (var enricher in enrichers)
        {
            enricher(context);
        }
    }
}
