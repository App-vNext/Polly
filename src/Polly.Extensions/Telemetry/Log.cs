using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters

[ExcludeFromCodeCoverage]
internal static partial class Log
{
    internal const string Separator = ", ";
    internal const string SourceWithStrategy = "Source: '{PipelineName}/{PipelineInstance}/{StrategyName}'";
    internal const string SourceWithoutStrategy = "Source: '{PipelineName}/{PipelineInstance}'";
    internal const string OperationKey = "Operation Key: '{OperationKey}'";
    internal const string Result = "Result: '{Result}'";
    internal const string ExecutionTime = "Execution Time: {ExecutionTimeMs}ms";

    [LoggerMessage(
        EventId = 0,
        Message = "Resilience event occurred. " +
                "EventName: '{EventName}', " +
                SourceWithStrategy + Separator +
                OperationKey + Separator +
                Result,
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
        SourceWithoutStrategy + Separator +
        OperationKey,
        EventName = "StrategyExecuting")]
    public static partial void PipelineExecuting(
        this ILogger logger,
        string pipelineName,
        string pipelineInstance,
        string? operationKey);

    [LoggerMessage(
        EventId = 2,
        Message = "Resilience pipeline executed. " +
            SourceWithoutStrategy + Separator +
            OperationKey + Separator +
            Result + Separator +
            ExecutionTime,
        EventName = "StrategyExecuted")]
    public static partial void PipelineExecuted(
        this ILogger logger,
        LogLevel logLevel,
        string pipelineName,
        string pipelineInstance,
        string? operationKey,
        object? result,
        double executionTimeMs,
        Exception? exception);

    [LoggerMessage(
        EventId = 3,
        Message = "Execution attempt. " +
                SourceWithStrategy + Separator +
                OperationKey + Separator +
                Result + Separator +
                "Handled: '{Handled}', " +
                "Attempt: '{Attempt}', " +
                ExecutionTime,
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
        double executionTimeMs,
        Exception? exception);
}
