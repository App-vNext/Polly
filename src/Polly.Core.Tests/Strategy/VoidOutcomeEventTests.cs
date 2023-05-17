using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class VoidOutcomeEventTests
{
    [Fact]
    public void IsEmpty_Ok()
    {
        var ev = new VoidOutcomeEvent<TestArguments>();

        ev.IsEmpty.Should().BeTrue();

        ev.Register(() => { });

        ev.IsEmpty.Should().BeFalse();
    }
}
