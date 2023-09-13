using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.Telemetry;

namespace Polly.Core.Tests;

public class StrategyBuilderContextTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var timeProvider = new FakeTimeProvider();
        var context = new StrategyBuilderContext(
            new ResilienceStrategyTelemetry(new ResilienceTelemetrySource("builder-name", "instance", "strategy_name"),
            Substitute.For<TelemetryListener>()), timeProvider);

        context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.PipelineInstanceName.Should().Be("instance");
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy_name");
        context.TimeProvider.Should().Be(timeProvider);
        context.Telemetry.Should().NotBeNull();

        context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy_name");
    }
}
