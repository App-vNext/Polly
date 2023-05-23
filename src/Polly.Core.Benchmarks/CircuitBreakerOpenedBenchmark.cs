using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Core.Benchmarks;

public class CircuitBreakerOpenedBenchmark
{
    private ResilienceStrategy? _strategy;
    private ResilienceStrategy? _strategyHandlesOutcome;
    private IAsyncPolicy<string>? _policy;

    [GlobalSetup]
    public void Setup()
    {
        _strategyHandlesOutcome = (ResilienceStrategy?)Helper.CreateOpenedCircuitBreaker(PollyVersion.V8, handleOutcome: true);
        _strategy = (ResilienceStrategy?)Helper.CreateOpenedCircuitBreaker(PollyVersion.V8, handleOutcome: false);
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
            await _strategy!.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
        }
        catch (BrokenCircuitException)
        {
            // ok
        }
    }

    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteAsync_Outcome_V8()
    {
        await _strategyHandlesOutcome!.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
    }
}
