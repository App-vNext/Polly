using BenchmarkDotNet.Attributes;
using Polly;

namespace Polly.Core.Benchmarks;

public class StrategyPipelineBenchmark
{
    private object? _strategyV7;
    private object? _strategyV8;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CreateStrategyPipeline(PollyVersion.V7);
        _strategyV8 = Helper.CreateStrategyPipeline(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteStrategyPipeline_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteStrategyPipeline_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);
}
