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

        context.Telemetry.TelemetrySource.PipelineName.ShouldBe("builder-name");
        context.Telemetry.TelemetrySource.PipelineInstanceName.ShouldBe("instance");
        context.Telemetry.TelemetrySource.StrategyName.ShouldBe("strategy_name");
        context.TimeProvider.ShouldBe(timeProvider);
        context.Telemetry.ShouldNotBeNull();

        context.Telemetry.TelemetrySource.PipelineName.ShouldBe("builder-name");
        context.Telemetry.TelemetrySource.StrategyName.ShouldBe("strategy_name");
    }
}
