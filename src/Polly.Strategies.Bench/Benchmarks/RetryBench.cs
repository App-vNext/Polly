using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks;

public class RetryBench
{
    private object? _strategy;

    [GlobalSetup]
    public void Setup() => _strategy = Helper.CreateRetries(PollyVersion);

    [Params(PollyVersion.V8, PollyVersion.V7)]
    public PollyVersion PollyVersion { get; set; }

    [Benchmark]
    public ValueTask ExecuteRetry() => _strategy!.ExecuteAsync(PollyVersion);
}
