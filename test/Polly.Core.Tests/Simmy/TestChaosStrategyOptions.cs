using Polly.Simmy;

namespace Polly.Core.Tests.Simmy;

public sealed class TestChaosStrategyOptions : MonkeyStrategyOptions
{
    public override string StrategyType => "Test";
}
