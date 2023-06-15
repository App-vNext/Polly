using Microsoft.Extensions.Logging;

namespace Polly.Extensions.Telemetry;

#pragma warning disable S107 // Methods should not have too many parameters
#pragma warning disable S109 // Magic numbers should not be used

#if NET6_0_OR_GREATER
internal static partial class Log
#else
internal static class Log
#endif
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

    private const string StrategyExecutingMessage = "Resilience strategy executing. " +
            "Builder Name: '{BuilderName}', " +
            "Strategy Key: '{StrategyKey}', " +
            "Result Type: '{ResultType}'";

#if NET6_0_OR_GREATER
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
#else
    private static readonly Action<ILogger, string, string?, string?, string, string?, object?, Exception?> ResilienceEventAction =
        LoggerMessage.Define<string, string?, string?, string, string?, object?>(LogLevel.Warning, new EventId(0, "ResilienceEvent"), ResilienceEventMessage);

    public static void ResilienceEvent(
        this ILogger logger,
        string eventName,
        string? builderName,
        string? strategyName,
        string strategyType,
        string? strategyKey,
        object? result,
        Exception? exception)
    {
        ResilienceEventAction(logger, eventName, builderName, strategyName, strategyType, strategyKey, result, exception);
    }
#endif

#if NET6_0_OR_GREATER
    [LoggerMessage(1, LogLevel.Debug, StrategyExecutingMessage, EventName = "StrategyExecuting")]
    public static partial void ExecutingStrategy(
        this ILogger logger,
        string? builderName,
        string? strategyKey,
        string resultType);
#else
    private static readonly Action<ILogger, string?, string?, string, Exception?> ExecutingStrategyAction =
        LoggerMessage.Define<string?, string?, string>(LogLevel.Debug, new EventId(1, "StrategyExecuting"), StrategyExecutingMessage);

    public static void ExecutingStrategy(
        this ILogger logger,
        string? builderName,
        string? strategyKey,
        string resultType)
    {
        ExecutingStrategyAction(logger, builderName, strategyKey, resultType, null);
    }
#endif

#if NET6_0_OR_GREATER
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
#else
    private static readonly Action<ILogger, string?, string?, string, object?, string, double, Exception?> StrategyExecutedActionDebug =
        LoggerMessage.Define<string?, string?, string, object?, string, double>(LogLevel.Debug, new EventId(2, "StrategyExecuted"), StrategyExecutedMessage);

    private static readonly Action<ILogger, string?, string?, string, object?, string, double, Exception?> StrategyExecutedActionWarning =
        LoggerMessage.Define<string?, string?, string, object?, string, double>(LogLevel.Warning, new EventId(2, "StrategyExecuted"), StrategyExecutedMessage);
    public static void StrategyExecuted(
        this ILogger logger,
        LogLevel logLevel,
        string? builderName,
        string? strategyKey,
        string resultType,
        object? result,
        string executionHealth,
        double executionTime,
        Exception? exception)
    {
        if (logLevel == LogLevel.Warning)
        {
            StrategyExecutedActionWarning(logger, builderName, strategyKey, resultType, result, executionHealth, executionTime, exception);
        }
        else
        {
            StrategyExecutedActionDebug(logger, builderName, strategyKey, resultType, result, executionHealth, executionTime, exception);
        }
    }
#endif
}
