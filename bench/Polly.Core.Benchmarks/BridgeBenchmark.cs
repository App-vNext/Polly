namespace Polly.Core.Benchmarks;

public class BridgeBenchmark
{
    private IAsyncPolicy<string>? _policy;
    private IAsyncPolicy<string>? _policyWrapped;

    [GlobalSetup]
    public void Setup()
    {
        _policy = Policy.NoOpAsync<string>();
        _policyWrapped = ResiliencePipeline<string>.Empty.AsAsyncPolicy();
    }

    [Benchmark(Baseline = true)]
    public Task NoOpAsync() => _policy!.ExecuteAsync(() => Task.FromResult("dummy"));

    [Benchmark]
    public Task NullResiliencePipeline() => _policyWrapped!.ExecuteAsync(() => Task.FromResult("dummy"));
}
