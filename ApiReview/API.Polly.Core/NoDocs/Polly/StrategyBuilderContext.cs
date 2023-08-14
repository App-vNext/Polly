// Assembly 'Polly.Core'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polly.Telemetry;

namespace Polly;

public sealed class StrategyBuilderContext
{
    public string? BuilderName { get; }
    public string? BuilderInstanceName { get; }
    public string? StrategyName { get; }
    public ResilienceStrategyTelemetry Telemetry { get; }
}
