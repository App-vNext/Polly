using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;
using Polly.Telemetry;

namespace Polly.Core.Benchmarks;

public class TelemetryBenchmark
{
    private ResilienceStrategy? _strategy;
    private MeterListener? _meterListener;

    [GlobalSetup]
    public void Prepare()
    {
        _strategy = Build(new ResilienceStrategyBuilder());

        if (Telemetry)
        {
            _meterListener = MeteringUtil.ListenPollyMetrics();
        }
    }

    [GlobalCleanup]
    public void Cleanup() => _meterListener?.Dispose();

    [Params(true, false)]
    public bool Telemetry { get; set; }

    [Params(true, false)]
    public bool Enrichment { get; set; }

    [Benchmark]
    public async ValueTask Execute()
    {
        var context = ResilienceContext.Get();
        await _strategy!.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
    }

    private ResilienceStrategy Build(ResilienceStrategyBuilder builder)
    {
        builder.AddStrategy(context => new TelemetryEventStrategy(context.Telemetry), new EmptyResilienceOptions());

        if (Telemetry)
        {
            TelemetryOptions options = new() { LoggerFactory = NullLoggerFactory.Instance };

            if (Enrichment)
            {
                options.Enrichers.Add(context =>
                {
                    // The Microsoft.Extensions.Resilience library will add around 6 additional tags
                    // https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.Resilience
                    context.Tags.Add(new("dummy1", "dummy"));
                    context.Tags.Add(new("dummy2", "dummy"));
                    context.Tags.Add(new("dummy3", "dummy"));
                    context.Tags.Add(new("dummy4", "dummy"));
                    context.Tags.Add(new("dummy5", "dummy"));
                    context.Tags.Add(new("dummy6", "dummy"));
                });
            }

            builder.ConfigureTelemetry(options);
        }

        return builder.Build();
    }

    private class TelemetryEventStrategy : ResilienceStrategy
    {
        private readonly ResilienceStrategyTelemetry _telemetry;

        public TelemetryEventStrategy(ResilienceStrategyTelemetry telemetry) => _telemetry = telemetry;

        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            _telemetry.Report(new ResilienceEvent(ResilienceEventSeverity.Warning, "DummyEvent"), context, "dummy-args");
            return callback(context, state);
        }
    }
}
