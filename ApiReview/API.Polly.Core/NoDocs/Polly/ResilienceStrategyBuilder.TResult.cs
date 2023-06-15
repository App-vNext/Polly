// Assembly 'Polly.Core'

namespace Polly;

public sealed class ResilienceStrategyBuilder<TResult> : ResilienceStrategyBuilderBase
{
    public ResilienceStrategyBuilder();
    public ResilienceStrategy<TResult> Build();
}
