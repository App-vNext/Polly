using Polly.Utils.Pipeline;

namespace Polly.Core.Benchmarks;

public class DelegatingComponentBenchmark : IAsyncDisposable
{
    private ResilienceContext? _context;
    private DelegatingComponent? _component;

    [GlobalSetup]
    public void Setup()
    {
        var first = PipelineComponent.Empty;
        var second = PipelineComponent.Empty;

        _component = new DelegatingComponent(first) { Next = second };
        _context = ResilienceContextPool.Shared.Get();
    }

    public async ValueTask DisposeAsync()
    {
        if (_component is not null)
        {
            await _component.DisposeAsync().ConfigureAwait(false);
            _component = null;
        }

        GC.SuppressFinalize(this);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<Outcome<int>> DelegatingComponent_ExecuteCore_Jit()
        => _component!.ExecuteComponent((_, state) => Outcome.FromResultAsValueTask(state), _context!, 42);

    [Benchmark]
    public ValueTask<Outcome<int>> DelegatingComponent_ExecuteCore_Aot()
        => _component!.ExecuteComponentAot((_, state) => Outcome.FromResultAsValueTask(state), _context!, 42);
}
