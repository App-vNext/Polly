using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters

[ExcludeFromCodeCoverage]
internal static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Message = "Resilience event occurred. " +
                "EventName: '{EventName}', " +
                "Source: '{BuilderName}/{BuilderInstance}/{StrategyName}', " +
                "Operation Key: '{OperationKey}', " +
                "Result: '{Result}'",
        EventName = "ResilienceEvent")]
    public static partial void ResilienceEvent(
        this ILogger logger,
        LogLevel logLevel,
        string eventName,
        string? builderName,
        string? builderInstance,
        string? strategyName,
        string? operationKey,
        object? result,
        Exception? exception);

    [LoggerMessage(
        1,
        LogLevel.Debug,
        "Resilience strategy executing. " +
        "Source: '{BuilderName}/{BuilderInstance}', " +
        "Operation Key: '{OperationKey}', " +
        "Result Type: '{ResultType}'",
        EventName = "StrategyExecuting")]
    public static partial void ExecutingStrategy(
        this ILogger logger,
        string? builderName,
        string? builderInstance,
        string? operationKey,
        string resultType);

    [LoggerMessage(
        EventId = 2,
        Message = "Resilience strategy executed. " +
            "Source: '{BuilderName}/{BuilderInstance}', " +
            "Operation Key: '{OperationKey}', " +
            "Result Type: '{ResultType}', " +
            "Result: '{Result}', " +
            "Execution Health: '{ExecutionHealth}', " +
            "Execution Time: {ExecutionTime}ms",
        EventName = "StrategyExecuted")]
    public static partial void StrategyExecuted(
        this ILogger logger,
        LogLevel logLevel,
        string? builderName,
        string? builderInstance,
        string? operationKey,
        string resultType,
        object? result,
        string executionHealth,
        double executionTime,
        Exception? exception);

    [LoggerMessage(
        EventId = 3,
        Message = "Execution attempt. " +
                "Source: '{BuilderName}/{BuilderInstance}/{StrategyName}', " +
                "Operation Key: '{OperationKey}', " +
                "Result: '{Result}', " +
                "Handled: '{Handled}', " +
                "Attempt: '{Attempt}', " +
                "Execution Time: '{ExecutionTimeMs}'",
        EventName = "ExecutionAttempt",
        SkipEnabledCheck = true)]
    public static partial void ExecutionAttempt(
        this ILogger logger,
        LogLevel level,
        string? builderName,
        string? builderInstance,
        string? strategyName,
        string? operationKey,
        object? result,
        bool handled,
        int attempt,
        double executionTimeMs,
        Exception? exception);
}
