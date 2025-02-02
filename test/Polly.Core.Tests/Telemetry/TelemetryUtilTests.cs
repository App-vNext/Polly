using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public static class TelemetryUtilTests
{
    [Theory]
    [InlineData(true, ResilienceEventSeverity.Warning)]
    [InlineData(false, ResilienceEventSeverity.Information)]
    public static void ReportExecutionAttempt_Ok(bool handled, ResilienceEventSeverity severity)
    {
        var asserted = false;
        var context = ResilienceContextPool.Shared.Get();
        var listener = TestUtilities.CreateResilienceTelemetry(args =>
        {
            args.Event.Severity.ShouldBe(severity);
            asserted = true;
        });

        TelemetryUtil.ReportExecutionAttempt(listener, context, Outcome.FromResult("dummy"), 0, TimeSpan.Zero, handled);
        asserted.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, ResilienceEventSeverity.Error)]
    [InlineData(false, ResilienceEventSeverity.Information)]
    public static void ReportFinalExecutionAttempt_Ok(bool handled, ResilienceEventSeverity severity)
    {
        var asserted = false;
        var context = ResilienceContextPool.Shared.Get();
        var listener = TestUtilities.CreateResilienceTelemetry(args =>
        {
            args.Event.Severity.ShouldBe(severity);
            asserted = true;
        });

        TelemetryUtil.ReportFinalExecutionAttempt(listener, context, Outcome.FromResult("dummy"), 1, TimeSpan.Zero, handled);
        asserted.ShouldBeTrue();
    }
}
