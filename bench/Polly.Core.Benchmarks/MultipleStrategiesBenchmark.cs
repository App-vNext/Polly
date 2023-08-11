using System.Diagnostics.Metrics;

namespace Polly.Core.Benchmarks;

public class MultipleStrategiesBenchmark
{
    private MeterListener? _meterListener;
    private object? _strategyV7;
    private object? _strategyV8;
    private object? _strategyTelemetryV8;
    private ResiliencePipeline? _nonGeneric;
    private ResiliencePipeline? _nonGenericTelemetry;

    [GlobalSetup]
    public void Setup()
    {
        _meterListener = MeteringUtil.ListenPollyMetrics();
        _strategyV7 = Helper.CreateStrategyPipeline(PollyVersion.V7, false);
        _strategyV8 = Helper.CreateStrategyPipeline(PollyVersion.V8, false);
        _strategyTelemetryV8 = Helper.CreateStrategyPipeline(PollyVersion.V8, true);
        _nonGeneric = Helper.CreateNonGenericStrategyPipeline(telemetry: false);
        _nonGenericTelemetry = Helper.CreateNonGenericStrategyPipeline(telemetry: true);
    }

    [GlobalCleanup]
    public void Cleanup() => _meterListener?.Dispose();

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteStrategyPipeline_Generic_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteStrategyPipeline_Generic_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);

    [Benchmark]
    public ValueTask ExecuteStrategyPipeline_GenericTelemetry_V8() => _strategyTelemetryV8!.ExecuteAsync(PollyVersion.V8);

    [Benchmark]
    public async ValueTask ExecuteStrategyPipeline_NonGeneric_V8()
    {
        var context = ResilienceContextPool.Shared.Get();

        await _nonGeneric!.ExecuteOutcomeAsync(
            static (_, _) => new ValueTask<Outcome<string>>(Outcome.FromResult("dummy")),
            context,
            string.Empty).ConfigureAwait(false);

        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteStrategyPipeline_NonGenericTelemetry_V8()
    {
        var context = ResilienceContextPool.Shared.Get();

        await _nonGenericTelemetry!.ExecuteOutcomeAsync(
            static (_, _) => new ValueTask<Outcome<string>>(Outcome.FromResult("dummy")),
            context,
            string.Empty).ConfigureAwait(false);

        ResilienceContextPool.Shared.Return(context);
    }
}
