namespace Polly.Core.Benchmarks.Utils;

internal sealed class EmptyResilienceOptions : ResilienceStrategyOptions
{
    public override string StrategyType => "Empty";
}
