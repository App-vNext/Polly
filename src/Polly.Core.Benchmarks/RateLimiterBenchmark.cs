using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Core.Benchmarks;

public class RateLimiterBenchmark
{
    private object? _strategyV7;
    private object? _strategyV8;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CreateRateLimiter(PollyVersion.V7);
        _strategyV8 = Helper.CreateRateLimiter(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteRateLimiter_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteRateLimiter_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);
}
