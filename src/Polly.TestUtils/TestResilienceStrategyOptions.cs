namespace Polly.TestUtils;

public sealed class TestResilienceStrategyOptions : ResilienceStrategyOptions
{
    public override string StrategyType => "Test";
}
