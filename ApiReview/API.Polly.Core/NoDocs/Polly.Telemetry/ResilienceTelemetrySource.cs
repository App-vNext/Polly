// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceTelemetrySource
{
    public string? PipelineName { get; }
    public string? PipelineInstanceName { get; }
    public string? StrategyName { get; }
    public ResilienceTelemetrySource(string? pipelineName, string? pipelineInstanceName, string? strategyName);
}
