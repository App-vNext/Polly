using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryResilienceStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new TelemetryResilienceStrategyOptions();

        options.Enrichers.Should().BeEmpty();
        options.LoggerFactory.Should().Be(NullLoggerFactory.Instance);
    }
}
