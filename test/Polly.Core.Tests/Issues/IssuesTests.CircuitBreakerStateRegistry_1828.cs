using Polly.CircuitBreaker;
using Polly.Registry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void CircuitBreakerStateRegistry_1828()
    {
        // Arrange
        var states = new ConcurrentBag<CircuitBreakerStateProvider>();
        using var registry = new ResiliencePipelineRegistry<string>();

        // Act
        _ = registry.GetOrAddPipeline("A", builder =>
        {
            var stateProvider = new CircuitBreakerStateProvider();
            builder.AddCircuitBreaker(new() { StateProvider = stateProvider });
            states.Add(stateProvider);
        });

        _ = registry.GetOrAddPipeline("B", builder =>
        {
            var stateProvider = new CircuitBreakerStateProvider();
            builder.AddCircuitBreaker(new() { StateProvider = stateProvider });
            states.Add(stateProvider);
        });

        _ = registry.TryAddBuilder("C", (builder, _) =>
        {
            var stateProvider = new CircuitBreakerStateProvider();
            builder.AddCircuitBreaker(new() { StateProvider = stateProvider });
            states.Add(stateProvider);
        });

        // Assert
        states.Count.ShouldBe(2);
        registry.GetPipeline("C");
        states.Count.ShouldBe(3);

        foreach (var state in states)
        {
            state.CircuitState.ShouldBe(CircuitState.Closed);
        }
    }
}
