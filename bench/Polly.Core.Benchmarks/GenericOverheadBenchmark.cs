using System.Runtime.CompilerServices;

namespace Polly.Core.Benchmarks;

public class GenericOverheadBenchmark
{
    private readonly GenericStrategy<string> _generic;
    private readonly NonGenericStrategy _nonGeneric;

    public GenericOverheadBenchmark()
    {
        _generic = new GenericStrategy<string>();
        _nonGeneric = new NonGenericStrategy();
    }

    [Benchmark(Baseline = true)]
    public async ValueTask ExecuteAsync_Generic() => await _generic.ExecuteAsync(static () => new ValueTask<string>("dummy")).ConfigureAwait(false);

    [Benchmark]
    public async ValueTask ExecuteAsync_NonGeneric() => await _nonGeneric.ExecuteAsync(static () => new ValueTask<string>("dummy")).ConfigureAwait(false);

    public class GenericStrategy<T>
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public virtual ValueTask<T> ExecuteAsync(Func<ValueTask<T>> callback)
        {
            return callback();
        }
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
