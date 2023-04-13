using Moq;
using Polly.Builder;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderContextTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var properties = new ResilienceProperties();
        var timeProvider = new FakeTimeProvider();
        var context = new ResilienceStrategyBuilderContext("builder-name", properties, "strategy-name", "strategy-type", timeProvider.Object);

        context.BuilderName.Should().Be("builder-name");
        context.BuilderProperties.Should().BeSameAs(properties);
        context.StrategyName.Should().Be("strategy-name");
        context.StrategyType.Should().Be("strategy-type");
        context.TimeProvider.Should().Be(timeProvider.Object);
        context.Telemetry.Should().NotBeNull();

        context.Telemetry.TelemetrySource.BuilderName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.BuilderProperties.Should().BeSameAs(properties);
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
        context.Telemetry.TelemetrySource.StrategyType.Should().Be("strategy-type");
    }
}
