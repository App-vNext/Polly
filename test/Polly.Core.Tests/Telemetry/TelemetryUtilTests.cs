using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class TelemetryUtilTests
{
    [InlineData(true, ResilienceEventSeverity.Warning)]
    [InlineData(false, ResilienceEventSeverity.Information)]
    [Theory]
    public void ReportExecutionAttempt_Ok(bool handled, ResilienceEventSeverity severity)
    {
        var asserted = false;
        var props = new ResilienceProperties();
        var listener = TestUtilities.CreateResilienceTelemetry(args =>
        {
            args.Event.Severity.Should().Be(severity);
            asserted = true;
        });

        TelemetryUtil.ReportExecutionAttempt(listener, ResilienceContextPool.Shared.Get(), Outcome.FromResult("dummy"), 0, TimeSpan.Zero, handled);
        asserted.Should().BeTrue();
    }
}
