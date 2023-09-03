using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Telemetry;

namespace Polly.Core.Benchmarks;

public class TelemetryBenchmark
{
    private ResiliencePipeline? _pipeline;
    private MeterListener? _meterListener;

    [GlobalSetup]
    public void Prepare()
    {
        _pipeline = Build(new ResiliencePipelineBuilder());

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
        var context = ResilienceContextPool.Shared.Get();
        await _pipeline!.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsValueTask("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    private ResiliencePipeline Build(ResiliencePipelineBuilder builder)
    {
        builder.AddStrategy(context => new TelemetryEventStrategy(context.Telemetry), new EmptyResilienceOptions());

        if (Telemetry)
        {
            TelemetryOptions options = new() { LoggerFactory = NullLoggerFactory.Instance };

            if (Enrichment)
            {
                options.MeteringEnrichers.Add(new CustomEnricher());
            }

            builder.ConfigureTelemetry(options);
        }

        return builder.Build();
    }

    private class CustomEnricher : MeteringEnricher
    {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            // The Microsoft.Extensions.Resilience library will add around 6 additional tags
            // https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.Resilience
            context.Tags.Add(new("dummy1", "dummy"));
            context.Tags.Add(new("dummy2", "dummy"));
            context.Tags.Add(new("dummy3", "dummy"));
            context.Tags.Add(new("dummy4", "dummy"));
            context.Tags.Add(new("dummy5", "dummy"));
            context.Tags.Add(new("dummy6", "dummy"));
        }
    }

    private class TelemetryEventStrategy : ResilienceStrategy
    {
        private readonly ResilienceStrategyTelemetry _telemetry;

        public TelemetryEventStrategy(ResilienceStrategyTelemetry telemetry) => _telemetry = telemetry;

        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            _telemetry.Report(new ResilienceEvent(ResilienceEventSeverity.Warning, "DummyEvent"), context, "dummy-args");
            return callback(context, state);
        }
    }
}
