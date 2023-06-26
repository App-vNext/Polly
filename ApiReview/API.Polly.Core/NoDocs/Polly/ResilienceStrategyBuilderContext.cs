// Assembly 'Polly.Core'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polly.Telemetry;

namespace Polly;

public sealed class ResilienceStrategyBuilderContext
{
    public string? BuilderName { get; }
    public ResilienceProperties BuilderProperties { get; }
    public string? StrategyName { get; }
    public string StrategyType { get; }
    public ResilienceStrategyTelemetry Telemetry { get; }
}
