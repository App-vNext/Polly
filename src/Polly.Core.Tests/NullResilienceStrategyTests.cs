namespace Polly.Core.Tests;

public class NullResilienceStrategyTests
{
    [Fact]
    public void Instance_ShouldNotBeNull()
    {
        NullResilienceStrategy.Instance.Should().NotBeNull();
        NullResilienceStrategy<string>.Instance.Should().NotBeNull();

    }

    [Fact]
    public void Execute_Ok()
    {
        bool executed = false;
        NullResilienceStrategy.Instance.Execute(_ => executed = true);
        executed.Should().BeTrue();

        NullResilienceStrategy<string>.Instance.Execute(_ => "res").Should().Be("res");
    }
}
