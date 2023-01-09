using BenchmarkDotNet.Attributes;
using Polly;

public class SimplePipelineBench
{
    private object? _strategy;

    [GlobalSetup]
    public void Setup() => _strategy = Helper.CraeteSimplePipeline(PollyVersion);

    [Params(PollyVersion.V8, PollyVersion.V7)]
    public PollyVersion PollyVersion { get; set; }

    [Benchmark]
    public ValueTask ExecuteSimplePipeline() => _strategy!.ExecuteAsync(PollyVersion);
}
