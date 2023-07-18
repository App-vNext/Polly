// Assembly 'Polly.Testing'

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polly.Testing;

public record InnerStrategiesDescriptor(IReadOnlyList<ResilienceStrategyDescriptor> Strategies, bool HasTelemetry, bool IsReloadable)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
