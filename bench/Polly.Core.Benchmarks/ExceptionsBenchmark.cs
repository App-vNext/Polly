namespace Polly.Core.Benchmarks;

[ExceptionDiagnoser]
public class ExceptionsBenchmark
{
    [Params(true, false)]
    public bool WithException { get; set; }

    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteOutcomeAsync()
    {
        var context = ResilienceContextPool.Shared.Get();
        await ResiliencePipeline.Empty.ExecuteOutcomeAsync(async (_, _) =>
        {
            // The callback for ExecuteOutcomeAsync must return an Outcome<T> instance. Hence, some wrapping is needed.
            try
            {
                return Outcome.FromResult(await ThrowAsync().ConfigureAwait(false));
            }
            catch (InvalidOperationException e)
            {
                return Outcome.FromException<int>(e);
            }
        }, context, 12).ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteOutcomeAsync_ImplicitOutcomeConversion()
    {
        var context = ResilienceContextPool.Shared.Get();
        await ResiliencePipeline.Empty.ExecuteOutcomeAsync((_, _) => ThrowAsync(), context, 12).ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    private async ValueTask<int> ThrowAsync()
    {
        await Task.Yield();
        return WithException ? throw new InvalidOperationException() : 12;
    }
}
