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
        if (!telemetry.Enabled)
        {
            return;
        }

        telemetry.Report(
            new(handled ? ResilienceEventSeverity.Warning : ResilienceEventSeverity.Information, ExecutionAttempt),
            context,
            outcome,
            new ExecutionAttemptArguments(attempt, executionTime, handled));
    }
}
