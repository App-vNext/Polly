// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Telemetry;

namespace Polly;

public sealed class StrategyBuilderContext
{
    public ResilienceStrategyTelemetry Telemetry { get; }
}
