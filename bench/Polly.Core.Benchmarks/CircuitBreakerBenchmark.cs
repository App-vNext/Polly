using BenchmarkDotNet.Attributes;

namespace Polly.Core.Benchmarks;

public class CircuitBreakerBenchmark
{
    private object? _strategyV7;
    private object? _strategyV8;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CreateCircuitBreaker(PollyVersion.V7);
        _strategyV8 = Helper.CreateCircuitBreaker(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteCircuitBreaker_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteCircuitBreaker_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);
}
