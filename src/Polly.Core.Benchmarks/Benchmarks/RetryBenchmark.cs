using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Polly.Core.Benchmarks;

namespace Polly.Benchmarks;

public class RetryBenchmark
{
    private object? _strategyV7;
    private object? _strategyV8;

    [GlobalSetup]
    public void Setup()
    {
        _strategyV7 = Helper.CreateRetry(PollyVersion.V7);
        _strategyV8 = Helper.CreateRetry(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteRetry_V7() => _strategyV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteRetry_V8() => _strategyV8!.ExecuteAsync(PollyVersion.V8);
}
