using Polly.Extensions.Telemetry;
using Polly.Extensions.Tests.Helpers;

namespace Polly.Extensions.Tests.Telemetry;

public class ResilienceTelemetryDiagnosticSourceTests
{
    [Fact]
    public void Meter_Ok()
    {
        ResilienceTelemetryDiagnosticSource.Meter.Name.Should().Be("Polly");
        ResilienceTelemetryDiagnosticSource.Meter.Version.Should().Be("1.0");
        new ResilienceTelemetryDiagnosticSource(new ResilienceStrategyTelemetryOptions())
            .Counter.Description.Should().Be("Tracks the number of resilience events that occurred in resilience strategies.");
    }

    [Fact]
    public void LoggerFactory_Ok()
    {
        var source = new ResilienceTelemetryDiagnosticSource(new ResilienceStrategyTelemetryOptions());
        source.LoggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void Write_InvalidType_Nothing()
    {
        var source = new ResilienceTelemetryDiagnosticSource(new ResilienceStrategyTelemetryOptions
        {
            LoggerFactory = TestUtils.CreateLoggerFactory(out var logger)
        });

        source.Write("dummy", new object());

        logger.Messages.Should().BeEmpty();
    }
}
