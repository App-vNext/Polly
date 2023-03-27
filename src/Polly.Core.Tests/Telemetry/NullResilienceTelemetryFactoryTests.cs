using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class NullResilienceTelemetryFactoryTests
{
    [Fact]
    public void Instance_NotNull()
    {
        NullResilienceTelemetry.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Default_Ok()
    {
        var context = new ResilienceTelemetryFactoryContext();

        context.BuilderName.Should().BeEmpty();
        context.StrategyType.Should().BeEmpty();
        context.StrategyName.Should().BeEmpty();
        context.BuilderProperties.Should().NotBeNull();
    }
}