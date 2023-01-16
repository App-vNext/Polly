using BenchmarkDotNet.Attributes;
using Polly;

public class SimplePipelineBench
{
    private object? _strategyV7;
    private object? _strategyV8;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CraeteSimplePipeline(PollyVersion.V7);
        _strategyV8 = Helper.CraeteSimplePipeline(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteSimplePipeline_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteSimplePipeline_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);
}
