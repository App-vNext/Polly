using System.Diagnostics.Metrics;

namespace Polly.Core.Benchmarks;

public class MultipleStrategiesBenchmark
{
    private MeterListener? _meterListener;
    private object? _strategyV7;
    private object? _strategyV8;
    private object? _strategyTelemetryV8;

    [GlobalSetup]
    public void Setup()
    {
        _meterListener = MeteringUtil.ListenPollyMetrics();
        _strategyV7 = Helper.CreateStrategyPipeline(PollyVersion.V7, false);
        _strategyV8 = Helper.CreateStrategyPipeline(PollyVersion.V8, false);
        _strategyTelemetryV8 = Helper.CreateStrategyPipeline(PollyVersion.V8, true);
    }

    [GlobalCleanup]
    public void Cleanup() => _meterListener?.Dispose();

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteStrategyPipeline_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteStrategyPipeline_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);

    [Benchmark]
    public ValueTask ExecuteStrategyPipeline_Telemetry_V8() => _strategyTelemetryV8!.ExecuteAsync(PollyVersion.V8);
}
