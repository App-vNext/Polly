using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters

internal static partial class Log
{
    private const string StrategyExecutedMessage = "Resilience strategy executed. " +
            "Builder Name: '{BuilderName}', " +
            "Strategy Key: '{StrategyKey}', " +
            "Result Type: '{ResultType}', " +
            "Result: '{Result}', " +
            "Execution Health: '{ExecutionHealth}', " +
            "Execution Time: {ExecutionTime}ms";

    private const string ResilienceEventMessage = "Resilience event occurred. " +
                "EventName: '{EventName}', " +
                "Builder Name: '{BuilderName}', " +
                "Strategy Name: '{StrategyName}', " +
                "Strategy Type: '{StrategyType}', " +
                "Strategy Key: '{StrategyKey}', " +
                "Result: '{Result}'";

    private const string ExecutionAttemptMessage = "Execution attempt. " +
                "Builder Name: '{BuilderName}', " +
                "Strategy Name: '{StrategyName}', " +
                "Strategy Type: '{StrategyType}', " +
                "Strategy Key: '{StrategyKey}', " +
                "Result: '{Result}', " +
                "Handled: '{Handled}', " +
                "Attempt: '{Attempt}', " +
                "Execution Time: '{ExecutionTimeMs}'";

    private const string StrategyExecutingMessage = "Resilience strategy executing. " +
            "Builder Name: '{BuilderName}', " +
            "Strategy Key: '{StrategyKey}', " +
            "Result Type: '{ResultType}'";

    [LoggerMessage(0, LogLevel.Warning, ResilienceEventMessage, EventName = "ResilienceEvent")]
    public static partial void ResilienceEvent(
        this ILogger logger,
        string eventName,
        string? builderName,
        string? strategyName,
        string strategyType,
        string? strategyKey,
        object? result,
        Exception? exception);

    [LoggerMessage(1, LogLevel.Debug, StrategyExecutingMessage, EventName = "StrategyExecuting")]
    public static partial void ExecutingStrategy(
        this ILogger logger,
        string? builderName,
        string? strategyKey,
        string resultType);

    [LoggerMessage(EventId = 2, Message = StrategyExecutedMessage, EventName = "StrategyExecuted")]
    public static partial void StrategyExecuted(
        this ILogger logger,
        LogLevel logLevel,
        string? builderName,
        string? strategyKey,
        string resultType,
        object? result,
        string executionHealth,
        double executionTime,
        Exception? exception);

    [LoggerMessage(EventId = 3, Message = ExecutionAttemptMessage, EventName = "ExecutionAttempt", SkipEnabledCheck = true)]
    public static partial void ExecutionAttempt(
        this ILogger logger,
        LogLevel level,
        string? builderName,
        string? strategyName,
        string strategyType,
        string? strategyKey,
        object? result,
        bool handled,
        int attempt,
        double executionTimeMs,
        Exception? exception);
}
