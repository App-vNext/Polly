using System.Runtime.CompilerServices;

namespace Polly.Core.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

public class ResilienceStrategyBenchmark
{
    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteOutcomeAsync()
    {
        var context = ResilienceContextPool.Shared.Get();
        await NullResilienceStrategy.Instance.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_ResilienceContextAndState()
    {
        var context = ResilienceContextPool.Shared.Get();
        await NullResilienceStrategy.Instance.ExecuteAsync((_, _) => new ValueTask<string>("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_CancellationToken()
    {
        await NullResilienceStrategy.Instance.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_GenericStrategy_CancellationToken()
    {
        await NullResilienceStrategy<string>.Instance.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
    }

    [Benchmark]
    public void Execute_ResilienceContextAndState()
    {
        var context = ResilienceContextPool.Shared.Get();
        NullResilienceStrategy.Instance.Execute((_, _) => "dummy", context, "state");
        ResilienceContextPool.Shared.Return(context);
    }

    [Benchmark]
    public void Execute_CancellationToken()
    {
        NullResilienceStrategy.Instance.Execute(_ => "dummy", CancellationToken.None);
    }

    [Benchmark]
    public void Execute_GenericStrategy_CancellationToken()
    {
        NullResilienceStrategy<string>.Instance.Execute(_ => "dummy", CancellationToken.None);
    }

    public class NonGenericStrategy
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public virtual ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> callback)
        {
            return callback();
        }
    }
}
