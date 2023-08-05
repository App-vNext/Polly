using Moq;
using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class TelemetryUtilTests
{
    [Fact]
    public void CreateResilienceTelemetry_Ok()
    {
        var telemetry = TelemetryUtil.CreateTelemetry(null, "builder", "instance", new ResilienceProperties(), "strategy-name");

        telemetry.TelemetrySource.BuilderName.Should().Be("builder");
        telemetry.TelemetrySource.BuilderInstanceName.Should().Be("instance");
        telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
        telemetry.DiagnosticSource.Should().BeNull();
    }

    [Fact]
    public void CreateResilienceTelemetry_DiagnosticSourceFromProperties_Ok()
    {
        var props = new ResilienceProperties();
        var source = Mock.Of<DiagnosticSource>();

        var telemetry = TelemetryUtil.CreateTelemetry(source, "builder", "instance", props, "strategy-name");

        telemetry.DiagnosticSource.Should().Be(source);
    }

    [InlineData(true, ResilienceEventSeverity.Warning)]
    [InlineData(false, ResilienceEventSeverity.Information)]
    [Theory]
    public void ReportExecutionAttempt_Ok(bool handled, ResilienceEventSeverity severity)
    {
        var asserted = false;
        var props = new ResilienceProperties();
        var telemetry = TestUtilities.CreateResilienceTelemetry(args =>
        {
            args.Event.Severity.Should().Be(severity);
            asserted = true;
        });

        TelemetryUtil.ReportExecutionAttempt(telemetry, ResilienceContextPool.Shared.Get(), Outcome.FromResult("dummy"), 0, TimeSpan.Zero, handled);
        asserted.Should().BeTrue();
    }
}
