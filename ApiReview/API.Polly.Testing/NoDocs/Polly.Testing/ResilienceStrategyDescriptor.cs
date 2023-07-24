// Assembly 'Polly.Testing'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Testing;

public sealed class ResilienceStrategyDescriptor
{
    public ResilienceStrategyOptions? Options { get; }
    public Type StrategyType { get; }
    public ResilienceStrategyDescriptor(ResilienceStrategyOptions? options, Type strategyType);
}
