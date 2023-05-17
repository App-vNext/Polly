using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class VoidOutcomePredicateTests
{
    [Fact]
    public void IsEmpty_Ok()
    {
        var ev = new VoidOutcomePredicate<TestArguments>();

        ev.IsEmpty.Should().BeTrue();

        ev.HandleException<InvalidOperationException>();

        ev.IsEmpty.Should().BeFalse();
    }
}
