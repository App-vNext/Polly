using FluentAssertions;
using Polly.Builder;
using Polly.Telemetry;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.Builder;

public class ResilienceStrategyBuilderOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new ResilienceStrategyBuilderOptions();

        options.BuilderName.Should().Be("");
        options.Properties.Should().NotBeNull();
        options.TimeProvider.Should().Be(TimeProvider.System);
        options.TelemetryFactory.Should().Be(NullResilienceTelemetryFactory.Instance);
    }
}
