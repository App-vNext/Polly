// Assembly 'Polly.Core'

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record ResilienceTelemetrySource(string? BuilderName, ResilienceProperties BuilderProperties, string? StrategyName, string StrategyType);
