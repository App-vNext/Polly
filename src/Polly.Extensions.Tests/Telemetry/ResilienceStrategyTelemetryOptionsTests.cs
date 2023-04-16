using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;
public class ResilienceStrategyTelemetryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new ResilienceStrategyTelemetryOptions();

        options.Enrichers.Should().BeEmpty();
        options.LoggerFactory.Should().Be(NullLoggerFactory.Instance);
        options.OutcomeFormatter(new Strategy.Outcome(typeof(string), "A")).Should().Be("A");
    }
}
