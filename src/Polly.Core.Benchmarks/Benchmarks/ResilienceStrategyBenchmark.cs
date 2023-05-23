using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Polly.Strategy;

namespace Polly.Benchmarks;

public class ResilienceStrategyBenchmark
{
    private readonly DummyResilienceStrategy _strategy = new();
    private readonly ResilienceStrategy<string> _genericStrategy = NullResilienceStrategy<string>.Instance;

    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteAsync_ResilienceContextAndState()
    {
        var context = ResilienceContext.Get();
        await _strategy.ExecuteAsync((_, _) => new ValueTask<Outcome<string>>(new Outcome<string>("dummy")), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_CancellationToken()
    {
        await _strategy.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
    }

    [Benchmark]
    public async ValueTask ExecuteAsync_GenericStrategy_CancellationToken()
    {
        await _genericStrategy.ExecuteAsync(_ => new ValueTask<string>("dummy"), CancellationToken.None).ConfigureAwait(false);
    }

    private class DummyResilienceStrategy : ResilienceStrategy
    {
        protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<TResult>> callback,
            ResilienceContext context,
            TState state) => callback(context, state);
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
