namespace Polly.Telemetry;

internal static class ResilienceTelemetryTags
{
    public const string EventName = "event.name";

    public const string EventSeverity = "event.severity";

    public const string PipelineName = "pipeline.name";

    public const string PipelineInstance = "pipeline.instance";

    public const string StrategyName = "strategy.name";

    public const string OperationKey = "operation.key";

    public const string ExceptionType = "exception.type";

    public const string AttemptNumber = "attempt.number";

    public const string AttemptHandled = "attempt.handled";
}
