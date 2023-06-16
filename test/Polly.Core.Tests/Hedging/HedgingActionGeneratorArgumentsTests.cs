using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingActionGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HedgingActionGeneratorArguments<string>(ResilienceContext.Get(), ResilienceContext.Get(), 5, _ => "dummy".AsOutcomeAsync());

        args.PrimaryContext.Should().NotBeNull();
        args.ActionContext.Should().NotBeNull();
        args.Attempt.Should().Be(5);
        args.Callback.Should().NotBeNull();
    }
}
