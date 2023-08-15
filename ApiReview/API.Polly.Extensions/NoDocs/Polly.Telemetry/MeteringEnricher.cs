// Assembly 'Polly.Extensions'

namespace Polly.Telemetry;

public abstract class MeteringEnricher
{
    public abstract void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context);
    protected MeteringEnricher();
}
