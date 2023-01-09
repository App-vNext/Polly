using BenchmarkDotNet.Attributes;
using Polly;

public class PipelineBench
{
    private object? _strategy;

    [GlobalSetup]
    public void Setup() => _strategy = Helper.CreatePipeline(PollyVersion, Components);

    [Params(2, 5, 10)]
    public int Components { get; set; }

    [Params(PollyVersion.V8, PollyVersion.V7)]
    public PollyVersion PollyVersion { get; set; }

    [Benchmark]
    public ValueTask ExecutePipeline() => _strategy!.ExecuteAsync(PollyVersion);
}
