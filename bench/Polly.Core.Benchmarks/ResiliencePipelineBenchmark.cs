using System.Runtime.CompilerServices;

namespace Polly.Core.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

public class ResiliencePipelineBenchmark
{
    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteOutcomeAsync()
    {
        var context = ResilienceContextPool.Shared.Get();
        await ResiliencePipeline.Empty.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsValueTask("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_ResilienceContextAndState()
    {
        var context = ResilienceContextPool.Shared.Get();
        await ResiliencePipeline.Empty.ExecuteAsync((_, _) => new ValueTask<string>("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_CancellationToken() =>
        await ResiliencePipeline.Empty.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);

    [Benchmark]
    public async ValueTask ExecuteAsync_GenericStrategy_CancellationToken() =>
        await ResiliencePipeline<string>.Empty.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);

    [Benchmark]
    public void Execute_ResilienceContextAndState()
    {
        var context = ResilienceContextPool.Shared.Get();
        ResiliencePipeline.Empty.Execute((_, _) => "dummy", context, "state");
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public void Execute_CancellationToken() =>
        ResiliencePipeline.Empty.Execute(_ => "dummy", CancellationToken.None);

    [Benchmark]
    public void Execute_GenericStrategy_CancellationToken() =>
        ResiliencePipeline<string>.Empty.Execute(_ => "dummy", CancellationToken.None);

    public class NonGenericStrategy
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public virtual ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> callback) => callback();
    }
}
