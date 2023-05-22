using Polly.Hedging;
using Polly.Hedging.Utils;
using Polly.Strategy;

namespace Polly.Core.Tests.Hedging.Controller;

public class HedgingControllerTests
{
    [Fact]
    public async Task Pooling_Ok()
    {
        var handler = new HedgingHandler().SetHedging<int>(handler => handler.HedgingActionGenerator = args => null).CreateHandler();
        var controller = new HedgingController(new HedgingTimeProvider(), handler!, 3);

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

    private static async Task PrepareAsync(HedgingExecutionContext context)
    {
        await context.LoadExecutionAsync((_, _) => new Outcome<int>(10).AsValueTask(), "state");
        await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);
        context.Tasks[0].AcceptOutcome();
    }
}
