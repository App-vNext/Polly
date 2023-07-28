// Assembly 'Polly.Core'

namespace Polly;

public sealed class CompositeStrategyBuilder : CompositeStrategyBuilderBase
{
    public ResilienceStrategy Build();
    public CompositeStrategyBuilder();
}
