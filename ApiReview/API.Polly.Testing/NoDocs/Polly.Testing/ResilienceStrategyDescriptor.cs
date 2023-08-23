// Assembly 'Polly.Testing'

using System.Runtime.CompilerServices;

namespace Polly.Testing;

public sealed class ResilienceStrategyDescriptor
{
    public ResilienceStrategyOptions? Options { get; }
    public object StrategyInstance { get; }
    public ResilienceStrategyDescriptor(ResilienceStrategyOptions? options, object strategyInstance);
}
