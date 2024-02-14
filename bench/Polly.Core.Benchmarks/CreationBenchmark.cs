namespace Polly.Core.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

public class CreationBenchmark
{
    [Benchmark]
    public static void Fallback_V7() =>
        Policy
            .HandleResult<string>(s => true)
            .FallbackAsync(_ => Task.FromResult("fallback"));

    [Benchmark]
    public static void Fallback_V8() =>
        new ResiliencePipelineBuilder<string>()
            .AddFallback(new()
            {
                FallbackAction = _ => Outcome.FromResultAsValueTask("fallback")
            })
            .Build();
}
