using Microsoft.Extensions.DependencyInjection;

namespace Polly.Core.Benchmarks;

public class ResiliencePipelineProviderBenchmark
{
    private ResiliencePipelineProvider<string>? _provider;

    [GlobalSetup]
    public void Setup() =>
        _provider = new ServiceCollection()
            .AddResiliencePipeline("dummy", builder => builder.AddTimeout(new TimeoutStrategyOptions()))
            .AddResiliencePipeline<string, string>("dummy", builder => builder.AddTimeout(new TimeoutStrategyOptions()))
            .BuildServiceProvider()
            .GetRequiredService<ResiliencePipelineProvider<string>>();

    [Benchmark]
    public void GetPipeline_Ok() => _provider!.GetPipeline("dummy");

    [Benchmark]
    public void GetPipeline_Generic_Ok() => _provider!.GetPipeline<string>("dummy");
}
