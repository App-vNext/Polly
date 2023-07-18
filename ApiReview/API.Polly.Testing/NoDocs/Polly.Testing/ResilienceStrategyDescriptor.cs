// Assembly 'Polly.Testing'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Testing;

public record ResilienceStrategyDescriptor(ResilienceStrategyOptions? Options, Type StrategyType)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
