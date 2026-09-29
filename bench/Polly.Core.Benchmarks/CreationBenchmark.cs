namespace Polly.Core.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
#pragma warning disable CA1822 // Member does not access instance data and can be made static

public class CreationBenchmark
{
    [Benchmark]
    public void Fallback_V7() =>
        Policy
            .HandleResult<string>(s => true)
            .FallbackAsync(_ => Task.FromResult("fallback"));

    [Benchmark]
    public void Fallback_V8() =>
        new ResiliencePipelineBuilder<string>()
            .AddFallback(new()
            {
                FallbackAction = _ => Outcome.FromResultAsValueTask("fallback")
            })
            .Build();
}
