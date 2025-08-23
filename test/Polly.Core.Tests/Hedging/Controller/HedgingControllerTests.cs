using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging.Controller;

public class HedgingControllerTests
{
    [Fact]
    public async Task Pooling_Ok()
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var telemetry = TestUtilities.CreateResilienceTelemetry(_ => { });
        var controller = new HedgingController<int>(telemetry, new HedgingTimeProvider(), HedgingHelper.CreateHandler<int>(_ => false, args => null), 3);

        var context1 = controller.GetContext(context);
        await PrepareAsync(context1);

        var context2 = controller.GetContext(context);
        await PrepareAsync(context2);

        controller.RentedContexts.ShouldBe(2);
        controller.RentedExecutions.ShouldBe(2);

        await context1.DisposeAsync();
        await context2.DisposeAsync();

        controller.RentedContexts.ShouldBe(0);
        controller.RentedExecutions.ShouldBe(0);
    }

    private static async Task PrepareAsync(HedgingExecutionContext<int> context)
    {
        await context.LoadExecutionAsync((_, _) => Outcome.FromResultAsValueTask(10), "state");
        await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);
        context.Tasks[0].AcceptOutcome();
    }
}
