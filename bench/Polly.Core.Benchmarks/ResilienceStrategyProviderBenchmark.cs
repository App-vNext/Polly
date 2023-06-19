using Microsoft.Extensions.DependencyInjection;

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
    public void Get_Ok() => _provider!.GetStrategy("dummy");

    [Benchmark]
    public void Get_Generic_Ok() => _provider!.GetStrategy<string>("dummy");
}
