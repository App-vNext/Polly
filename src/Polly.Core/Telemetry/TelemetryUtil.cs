namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal const string ExecutionAttempt = "ExecutionAttempt";

    public static ResilienceStrategyTelemetry CreateTelemetry(
        DiagnosticSource? diagnosticSource,
        string? builderName,
        string? builderInstanceName,
        ResilienceProperties builderProperties,
        string? strategyName)
    {
        var telemetrySource = new ResilienceTelemetrySource(builderName, builderInstanceName, builderProperties, strategyName);

        return new ResilienceStrategyTelemetry(telemetrySource, diagnosticSource);
    }

    public static void ReportExecutionAttempt<TResult>(
        ResilienceStrategyTelemetry telemetry,
        ResilienceContext context,
        Outcome<TResult> outcome,
        int attempt,
        TimeSpan executionTime,
        bool handled)
    {
        if (!telemetry.IsEnabled)
        {
            return;
        }

        var attemptArgs = ExecutionAttemptArguments.Get(attempt, executionTime, handled);
        telemetry.Report<ExecutionAttemptArguments, TResult>(
            new(handled ? ResilienceEventSeverity.Warning : ResilienceEventSeverity.Information, ExecutionAttempt),
            new(context, outcome, attemptArgs));
        ExecutionAttemptArguments.Return(attemptArgs);
    }
}
