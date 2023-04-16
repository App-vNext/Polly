using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Extensions.Telemetry;

internal class ResilienceTelemetryDiagnosticSource : DiagnosticSource
{
    private const string ResilienceEventFormatString =
        "Resilience event occurred. " +
        "EventName: '{EventName}', " +
        "Builder Name: '{BuilderName}', " +
        "Strategy Name: '{StrategyName}', " +
        "Strategy Type: '{StrategyType}', " +
        "Strategy Key: '{StrategyKey}', " +
        "Outcome: '{Outcome}'";

    internal static readonly Meter Meter = new(TelemetryUtil.PollyDiagnosticSource, "1.0");

    private static readonly Action<ILogger, string, string, string, string, string, string, Exception?> ResilienceEventLog = LoggerMessage.Define<string, string, string, string, string, string>(
            LogLevel.Warning,
            new EventId(1, "ResilienceEvent"),
            ResilienceEventFormatString);

    private readonly ILogger _logger;

    public ResilienceTelemetryDiagnosticSource(ResilienceStrategyTelemetryOptions options)
    {
        LoggerFactory = options.LoggerFactory;
        Enrichers = options.Enrichers.ToList();
        OutcomeFormatter = options.OutcomeFormatter;

        _logger = options.LoggerFactory.CreateLogger(TelemetryUtil.PollyDiagnosticSource);
        Counter = Meter.CreateCounter<int>(
            "resilience-events",
            description: "Tracks the number of resilience events that occurred in resilience strategies.");
    }

    public ILoggerFactory LoggerFactory { get; }

    public Func<Outcome, string> OutcomeFormatter { get; }

    public List<Action<EnrichmentContext>> Enrichers { get; }

    public Counter<int> Counter { get; }

    public override bool IsEnabled(string name) => true;

    public override void Write(string name, object? value)
    {
        if (value is not TelemetryEventArguments args)
        {
            return;
        }

        LogEvent(args);
        MeterEvent(args);
    }

    private void MeterEvent(TelemetryEventArguments args)
    {
        var tags = new TagList
        {
            { "event-name", args.EventName },
            { "builder-name", args.Source.BuilderName },
            { "strategy-name", args.Source.StrategyName },
            { "strategy-type", args.Source.StrategyType },
            { "result-type", args.Context.IsVoid ? "void" : args.Context.ResultType.Name.ToString(CultureInfo.InvariantCulture) }
        };

        if (args.Source.BuilderProperties.TryGetValue(TelemetryUtil.StrategyKey, out var key))
        {
            tags.Add("strategy-key", key);
        }
        else
        {
            tags.Add("strategy-key", null);
        }

        tags.Add("exception-name", args.Outcome?.Exception?.GetType().FullName);

        Enrich(args, ref tags);

        Counter.Add(1, tags);
    }

    private void Enrich(TelemetryEventArguments args, ref TagList tags)
    {
        if (Enrichers.Count == 0)
        {
            return;
        }

        var context = EnrichmentContext.Get(args.Arguments, args.Outcome);

        foreach (var enricher in Enrichers)
        {
            enricher(context);
        }

        foreach (var pair in context.Tags)
        {
            tags.Add(pair.Key, pair.Value);
        }

        EnrichmentContext.Return(context);
    }

    private void LogEvent(TelemetryEventArguments args)
    {
        string outcomeString = "null";

        if (args.Outcome is Outcome outcome)
        {
            outcomeString = OutcomeFormatter(outcome);
        }

        if (!args.Source.BuilderProperties.TryGetValue(TelemetryUtil.StrategyKey, out var strategyKey))
        {
            strategyKey = "null";
        }

        if (args.Outcome?.Exception is Exception exception)
        {
            ResilienceEventLog(_logger, args.EventName, args.Source.BuilderName, args.Source.StrategyName, args.Source.StrategyType, strategyKey, outcomeString, exception);
        }
        else
        {
            ResilienceEventLog(_logger, args.EventName, args.Source.BuilderName, args.Source.StrategyName, args.Source.StrategyType, strategyKey, outcomeString, null);
        }
    }
}
