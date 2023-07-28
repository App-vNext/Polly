// Assembly 'Polly.Core'

namespace Polly;

public sealed class CompositeStrategyBuilder<TResult> : CompositeStrategyBuilderBase
{
    public CompositeStrategyBuilder();
    public ResilienceStrategy<TResult> Build();
}
