using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Core.Benchmarks;

public class CompositeComponentBenchmark
{
    private ResilienceContext? _context;
    private PipelineComponent? _component;

    [GlobalSetup]
    public void Setup()
    {
        var first = PipelineComponent.Empty;
        var second = PipelineComponent.Empty;
        var source = new ResilienceTelemetrySource("pipeline", "instance", "strategy");
        var telemetry = new ResilienceStrategyTelemetry(source, null);
        var components = new[] { first, second };

        _component = CompositeComponent.Create(components, telemetry, TimeProvider.System);
        _context = ResilienceContextPool.Shared.Get();
    }

    [Benchmark]
    public ValueTask<Outcome<int>> CompositeComponent_ExecuteCore()
        => _component!.ExecuteCore((_, state) => Outcome.FromResultAsValueTask(state), _context!, 42);
}
