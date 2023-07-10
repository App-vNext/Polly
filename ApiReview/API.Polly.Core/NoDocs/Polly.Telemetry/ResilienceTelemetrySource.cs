// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed record ResilienceTelemetrySource(string? BuilderName, string? BuilderInstanceName, ResilienceProperties BuilderProperties, string? StrategyName, string StrategyType);
