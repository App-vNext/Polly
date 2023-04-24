using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class VoidOutcomeGeneratorTests
{
    [Fact]
    public void IsEmpty_Ok()
    {
        var ev = new VoidOutcomeGenerator<TestArguments, int>();

        ev.IsEmpty.Should().BeTrue();

        ev.SetGenerator((_, _) => 10);

        ev.IsEmpty.Should().BeFalse();
    }
}
