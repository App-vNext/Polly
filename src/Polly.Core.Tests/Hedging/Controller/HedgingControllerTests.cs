using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging.Controller;

public class HedgingControllerTests
{
    [Fact]
    public async Task Pooling_Ok()
    {
        var controller = new HedgingController<int>(new HedgingTimeProvider(), HedgingHelper.CreateHandler<int>(_ => false, args => null), 3);

        var context1 = controller.GetContext(ResilienceContext.Get());
        await PrepareAsync(context1);

        var context2 = controller.GetContext(ResilienceContext.Get());
        await PrepareAsync(context2);

        controller.RentedContexts.Should().Be(2);
        controller.RentedExecutions.Should().Be(2);

        context1.Complete();
        context2.Complete();

        controller.RentedContexts.Should().Be(0);
        controller.RentedExecutions.Should().Be(0);
    }

    private static async Task PrepareAsync(HedgingExecutionContext<int> context)
    {
        await context.LoadExecutionAsync((_, _) => new Outcome<int>(10).AsValueTask(), "state");
        await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);
        context.Tasks[0].AcceptOutcome();
    }
}
