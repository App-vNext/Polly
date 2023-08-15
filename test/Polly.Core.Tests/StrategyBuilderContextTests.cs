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
        var context = new StrategyBuilderContext("builder-name", "instance", "strategy-name", timeProvider, Substitute.For<TelemetryListener>());

        context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.PipelineInstanceName.Should().Be("instance");
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
        context.TimeProvider.Should().Be(timeProvider);
        context.Telemetry.Should().NotBeNull();

        context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
    }
}
