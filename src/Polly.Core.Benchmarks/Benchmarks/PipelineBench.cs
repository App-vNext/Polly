using BenchmarkDotNet.Attributes;
using Polly;

namespace Polly.Core.Benchmarks;

public class PipelineBench
{
    private object? _strategyV7;
    private object? _strategyV8;
    private object? _strategyV8Delegating;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CreatePipeline(PollyVersion.V7, Components, false);
        _strategyV8 = Helper.CreatePipeline(PollyVersion.V8, Components, false);
        _strategyV8Delegating = Helper.CreatePipeline(PollyVersion.V8, Components, true);
    }

    [Params(1, 2, 5, 10)]
    public int Components { get; set; }

    [Benchmark(Baseline = true)]
    public ValueTask ExecutePipeline_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecutePipeline_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);

    [Benchmark]
    public ValueTask ExecutePipeline_V8Delegating() => _strategyV8Delegating!.ExecuteAsync(PollyVersion.V8);
}
