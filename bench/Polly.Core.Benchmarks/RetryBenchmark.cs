namespace Polly.Core.Benchmarks;

public class RetryBenchmark
{
    private object? _retryV7;
    private object? _retryV8;

    [GlobalSetup]
    public void Setup()
    {
        _retryV7 = Helper.CreateRetry(PollyVersion.V7);
        _retryV8 = Helper.CreateRetry(PollyVersion.V8);
    }

    [Benchmark(Baseline = true)]
    public ValueTask ExecuteRetry_V7() => _retryV7!.ExecuteAsync(PollyVersion.V7);

    [Benchmark]
    public ValueTask ExecuteRetry_V8() => _retryV8!.ExecuteAsync(PollyVersion.V8);
}
