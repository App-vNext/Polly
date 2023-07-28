using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Polly.Core.Tests;

public class CompositeStrategyBuilderContextTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var properties = new ResilienceProperties();
        var timeProvider = new FakeTimeProvider();
        var context = new StrategyBuilderContext("builder-name", "instance", properties, "strategy-name", timeProvider, Mock.Of<DiagnosticSource>(), () => 1.0);

        context.BuilderName.Should().Be("builder-name");
        context.BuilderInstanceName.Should().Be("instance");
        context.BuilderProperties.Should().BeSameAs(properties);
        context.StrategyName.Should().Be("strategy-name");
        context.TimeProvider.Should().Be(timeProvider);
        context.Telemetry.Should().NotBeNull();
        context.Randomizer.Should().NotBeNull();

        context.Telemetry.TelemetrySource.BuilderName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.BuilderProperties.Should().BeSameAs(properties);
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
    }
}
