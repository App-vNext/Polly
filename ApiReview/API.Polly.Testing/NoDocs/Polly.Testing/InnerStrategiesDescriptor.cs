// Assembly 'Polly.Testing'

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polly.Testing;

public sealed class InnerStrategiesDescriptor
{
    public IReadOnlyList<ResilienceStrategyDescriptor> Strategies { get; }
    public bool HasTelemetry { get; }
    public bool IsReloadable { get; }
    public InnerStrategiesDescriptor(IReadOnlyList<ResilienceStrategyDescriptor> strategies, bool hasTelemetry, bool isReloadable);
}
