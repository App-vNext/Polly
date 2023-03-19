using FluentAssertions;
using Polly.Builder;
using Polly.Telemetry;
using Xunit;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderContextTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var context = new ResilienceStrategyBuilderContext();

        context.BuilderName.Should().Be("");
        context.BuilderProperties.Should().NotBeNull();
        context.StrategyName.Should().Be("");
        context.StrategyType.Should().Be("");
        context.Telemetry.Should().Be(NullResilienceTelemetry.Instance);
    }
}