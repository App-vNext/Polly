using BenchmarkDotNet.Attributes;
using Polly;

public class TimeoutBench
{
    private object? _strategy;

    [GlobalSetup]
    public void Setup() => _strategy = Helper.CreateTimeout(PollyVersion);

    [Params(PollyVersion.V8, PollyVersion.V7)]
    public PollyVersion PollyVersion { get; set; }

    [Benchmark]
    public ValueTask ExecuteTimeout() => _strategy!.ExecuteAsync(PollyVersion);
}
