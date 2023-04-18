using System;
using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters
#pragma warning disable SA1118 // Parameter should not span multiple lines

internal static class Log
{
    private static readonly Action<ILogger, string, string, string, string, string?, object?, Exception?> ResilienceEventAction =
        LoggerMessage.Define<string, string, string, string, string?, object?>(
            LogLevel.Warning,
            new EventId(0, "ResilienceEvent"),
            "Resilience event occurred. " +
            "EventName: '{EventName}', " +
            "Builder Name: '{BuilderName}', " +
            "Strategy Name: '{StrategyName}', " +
            "Strategy Type: '{StrategyType}', " +
            "Strategy Key: '{StrategyKey}', " +
            "Result: '{Result}'");

    private static readonly Action<ILogger, string, string?, string, Exception?> ExecutingStrategyAction = LoggerMessage.Define<string, string?, string>(
        LogLevel.Debug,
        new EventId(1, "ExecutingStrategy"),
        "Resilience strategy executing. " +
        "Builder Name: '{BuilderName}', " +
        "Strategy Key: '{StrategyKey}', " +
        "Result Type: '{ResultType}'");

    private static readonly Action<ILogger, string, string?, string, object?, string, double, Exception?> StrategyExecutedAction =
        LoggerMessage.Define<string, string?, string, object?, string, double>(
            LogLevel.Debug,
            new EventId(2, "StrategyExecuted"),
            "Resilience strategy executed. " +
            "Builder Name: '{BuilderName}', " +
            "Strategy Key: '{StrategyKey}', " +
            "Result Type: '{ResultType}', " +
            "Result: '{Result}', " +
            "Execution Health: '{ExecutionHealth}', " +
            "Execution Time: {ExecutionTime}ms");

    public static void ResilienceEvent(
        this ILogger logger,
        string eventName,
        string builderName,
        string strategyName,
        string strategyType,
        string? strategyKey,
        object? outcome,
        Exception? exception)
    {
        ResilienceEventAction(logger, eventName, builderName, strategyName, strategyType, strategyKey, outcome, exception);
    }

    public static void ExecutingStrategy(
        this ILogger logger,
        string builderName,
        string? strategyKey,
        string resultType)
    {
        ExecutingStrategyAction(logger, builderName, strategyKey, resultType, null);
    }

    public static void StrategyExecuted(
        this ILogger logger,
        string builderName,
        string? strategyKey,
        string resultType,
        object? result,
        string executionHealth,
        double executionTime,
        Exception? exception)
    {
        StrategyExecutedAction(logger, builderName, strategyKey, resultType, result, executionHealth, executionTime, exception);
    }
}
