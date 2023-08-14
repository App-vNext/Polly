// Assembly 'Polly.Testing'

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polly.Testing;

public sealed class ResiliencePipelineDescriptor
{
    public IReadOnlyList<ResilienceStrategyDescriptor> Strategies { get; }
    public ResilienceStrategyDescriptor FirstStrategy { get; }
    public bool IsReloadable { get; }
    public ResiliencePipelineDescriptor(IReadOnlyList<ResilienceStrategyDescriptor> strategies, bool isReloadable);
}
