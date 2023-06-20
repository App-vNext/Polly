using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Core.Tests.Hedging.Controller;

public class HedgingControllerTests
{
    [Fact]
    public async Task Pooling_Ok()
    {
        var telemetry = TestUtilities.CreateResilienceTelemetry(_ => { });
        var controller = new HedgingController<int>(telemetry, new HedgingTimeProvider(), HedgingHelper.CreateHandler<int>(_ => false, args => null), 3);

        var context1 = controller.GetContext(ResilienceContext.Get());
        await PrepareAsync(context1);

        var context2 = controller.GetContext(ResilienceContext.Get());
        await PrepareAsync(context2);

        controller.RentedContexts.Should().Be(2);
        controller.RentedExecutions.Should().Be(2);

        await context1.DisposeAsync();
        await context2.DisposeAsync();

        controller.RentedContexts.Should().Be(0);
        controller.RentedExecutions.Should().Be(0);
    }

    private static async Task PrepareAsync(HedgingExecutionContext<int> context)
    {
        await context.LoadExecutionAsync((_, _) => Outcome.FromResultAsTask(10), "state");
        await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);
        context.Tasks[0].AcceptOutcome();
    }
}
