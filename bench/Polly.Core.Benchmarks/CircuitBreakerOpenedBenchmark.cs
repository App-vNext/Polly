namespace Polly.Core.Benchmarks;

public class CircuitBreakerOpenedBenchmark
{
    private ResiliencePipeline? _pipeline;
    private ResiliencePipeline? _reactivePipeline;
    private IAsyncPolicy<string>? _policy;

    [GlobalSetup]
    public void Setup()
    {
        _reactivePipeline = (ResiliencePipeline?)Helper.CreateOpenedCircuitBreaker(PollyVersion.V8, handleOutcome: true);
        _pipeline = (ResiliencePipeline?)Helper.CreateOpenedCircuitBreaker(PollyVersion.V8, handleOutcome: false);
        _policy = (IAsyncPolicy<string>?)Helper.CreateOpenedCircuitBreaker(PollyVersion.V7, handleOutcome: false);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_Exception_V7()
    {
        try
        {
            await _policy!.ExecuteAsync(_ => Task.FromResult("dummy"), CancellationToken.None).ConfigureAwait(false);
        }
        catch (BrokenCircuitException)
        {
            // ok
        }
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_Exception_V8()
    {
        try
        {
            await _pipeline!.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
        }
        catch (BrokenCircuitException)
        {
            // ok
        }
    }

    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteAsync_Outcome_V8()
        => await _reactivePipeline!.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
}
