using Polly.Telemetry;

namespace Polly.Extensions.Tests.Utils;

public class ResilienceContextExtensionsTests
{
    [Fact]
    public void IsHealthy_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        AddEvent(context, ResilienceEventSeverity.Warning);
        context.IsExecutionHealthy().Should().BeFalse();

        context = ResilienceContextPool.Shared.Get();
        context.IsExecutionHealthy().Should().BeTrue();

        context = ResilienceContextPool.Shared.Get();
        AddEvent(context, ResilienceEventSeverity.Information);
        context.IsExecutionHealthy().Should().BeTrue();

        context = ResilienceContextPool.Shared.Get();
        AddEvent(context, ResilienceEventSeverity.Information);
        AddEvent(context, ResilienceEventSeverity.Warning);
        context.IsExecutionHealthy().Should().BeFalse();
    }

    private static void AddEvent(ResilienceContext context, ResilienceEventSeverity severity)
    {
        ((List<ResilienceEvent>)context.ResilienceEvents).Add(new ResilienceEvent(severity, "dummy"));
    }
}
