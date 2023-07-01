namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal const string ExecutionAttempt = "ExecutionAttempt";

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("Polly.StrategyKey");

    public static ResilienceStrategyTelemetry CreateTelemetry(
        DiagnosticSource? diagnosticSource,
        string? builderName,
        ResilienceProperties builderProperties,
        string? strategyName,
        string strategyType)
    {
        var telemetrySource = new ResilienceTelemetrySource(builderName, builderProperties, strategyName, strategyType);

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
