namespace Polly.Core.Tests;

public class ResilienceStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new TestResilienceStrategyOptions();

        options.StrategyType.Should().Be("Test");
        options.StrategyName.Should().BeNull();
    }
}
