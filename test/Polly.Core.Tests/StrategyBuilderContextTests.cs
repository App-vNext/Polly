using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Polly.Core.Tests;

public class StrategyBuilderContextTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var timeProvider = new FakeTimeProvider();
        var context = new StrategyBuilderContext("builder-name", "instance", "strategy-name", timeProvider, Substitute.For<DiagnosticSource>());

        context.BuilderName.Should().Be("builder-name");
        context.BuilderInstanceName.Should().Be("instance");
        context.StrategyName.Should().Be("strategy-name");
        context.TimeProvider.Should().Be(timeProvider);
        context.Telemetry.Should().NotBeNull();

        context.Telemetry.TelemetrySource.PipelineName.Should().Be("builder-name");
        context.Telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
    }
}
