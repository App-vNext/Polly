namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal const string ExecutionAttempt = "ExecutionAttempt";

    internal const string PipelineExecuting = "PipelineExecuting";

    internal const string PipelineExecuted = "PipelineExecuted";

    public static void ReportExecutionAttempt<TResult>(
        ResilienceStrategyTelemetry telemetry,
        ResilienceContext context,
        Outcome<TResult> outcome,
        int attempt,
        TimeSpan executionTime,
        bool handled)
    {
        telemetry.Report<ExecutionAttemptArguments, TResult>(
            new(handled ? ResilienceEventSeverity.Warning : ResilienceEventSeverity.Information, ExecutionAttempt),
            new(context, outcome, new ExecutionAttemptArguments(attempt, executionTime, handled)));
    }
}
