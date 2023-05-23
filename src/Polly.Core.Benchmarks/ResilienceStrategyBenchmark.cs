using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Polly.Strategy;

namespace Polly.Core.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

public class ResilienceStrategyBenchmark
{
    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteOutcomeAsync()
    {
        var context = ResilienceContext.Get();
        await NullResilienceStrategy.Instance.ExecuteOutcomeAsync((_, _) => new ValueTask<Outcome<string>>(new Outcome<string>("dummy")), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_ResilienceContextAndState()
    {
        var context = ResilienceContext.Get();
        await NullResilienceStrategy.Instance.ExecuteAsync((_, _) => new ValueTask<string>("dummy"), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
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

    public class NonGenericStrategy
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public virtual ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> callback)
        {
            return callback();
        }
    }
}
