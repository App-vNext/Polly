using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Polly.Core.Benchmarks;

namespace Polly.Core.Benchmarks;

public class ResilienceStrategyProviderBenchmark
{
    private ResilienceStrategyProvider<string>? _provider;

    [GlobalSetup]
    public void Setup()
    {
        _provider = new ServiceCollection()
            .AddResilienceStrategy("dummy", builder => builder.AddTimeout(new TimeoutStrategyOptions()))
            .AddResilienceStrategy<string, string>("dummy", builder => builder.AddTimeout(new TimeoutStrategyOptions()))
            .BuildServiceProvider()
            .GetRequiredService<ResilienceStrategyProvider<string>>();
    }

    [Benchmark]
    public void Get_Ok() => _provider!.Get("dummy");

    [Benchmark]
    public void Get_Generic_Ok() => _provider!.Get<string>("dummy");
}
