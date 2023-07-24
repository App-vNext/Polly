// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceTelemetrySource
{
    public string? BuilderName { get; }
    public string? BuilderInstanceName { get; }
    public ResilienceProperties BuilderProperties { get; }
    public string? StrategyName { get; }
    public ResilienceTelemetrySource(string? builderName, string? builderInstanceName, ResilienceProperties builderProperties, string? strategyName);
}
