using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters

[ExcludeFromCodeCoverage]
internal static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Message = "Resilience event occurred. " +
                "EventName: '{EventName}', " +
                "Source: '{PipelineName}/{PipelineInstance}/{StrategyName}', " +
                "Operation Key: '{OperationKey}', " +
                "Result: '{Result}'",
        EventName = "ResilienceEvent")]
    public static partial void ResilienceEvent(
        this ILogger logger,
        LogLevel logLevel,
        string eventName,
        string pipelineName,
        string pipelineInstance,
        string? strategyName,
        string? operationKey,
        object? result,
        Exception? exception);

    [LoggerMessage(
        1,
        LogLevel.Debug,
        "Resilience pipeline executing. " +
        "Source: '{PipelineName}/{PipelineInstance}', " +
        "Operation Key: '{OperationKey}'",
        EventName = "StrategyExecuting")]
    public static partial void PipelineExecuting(
        this ILogger logger,
        string pipelineName,
        string pipelineInstance,
        string? operationKey);

    [LoggerMessage(
        EventId = 2,
        Message = "Resilience pipeline executed. " +
            "Source: '{PipelineName}/{PipelineInstance}', " +
            "Operation Key: '{OperationKey}', " +
            "Result: '{Result}', " +
            "Execution Time: {ExecutionTime}ms",
        EventName = "StrategyExecuted")]
    public static partial void PipelineExecuted(
        this ILogger logger,
        LogLevel logLevel,
        string pipelineName,
        string pipelineInstance,
        string? operationKey,
        object? result,
        double executionTime,
        Exception? exception);

    [LoggerMessage(
        EventId = 3,
        Message = "Execution attempt. " +
                "Source: '{PipelineName}/{PipelineInstance}/{StrategyName}', " +
                "Operation Key: '{OperationKey}', " +
                "Result: '{Result}', " +
                "Handled: '{Handled}', " +
                "Attempt: '{Attempt}', " +
                "Execution Time: {ExecutionTime}ms",
        EventName = "ExecutionAttempt",
        SkipEnabledCheck = true)]
    public static partial void ExecutionAttempt(
        this ILogger logger,
        LogLevel level,
        string pipelineName,
        string pipelineInstance,
        string strategyName,
        string? operationKey,
        object? result,
        bool handled,
        int attempt,
        double executionTime,
        Exception? exception);
}
