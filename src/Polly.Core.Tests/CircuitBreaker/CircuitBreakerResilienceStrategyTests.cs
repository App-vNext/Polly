using Moq;
using Polly.Strategy;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResilienceStrategyTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public CircuitBreakerResilienceStrategyTests()
    {
        _timeProvider = new FakeTimeProvider();
        _telemetry = TestUtilities.CreateResilienceTelemetry(Mock.Of<DiagnosticSource>());
    }

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public void Execute_Ok()
    {
        Create().Invoking(s => s.Execute(_ => { })).Should().NotThrow();
    }

    private CircuitBreakerResilienceStrategy Create() => new(_timeProvider.Object, _telemetry);
}
