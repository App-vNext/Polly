using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyTests
{
    private readonly HedgingStrategyOptions _options = new();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var strategy = Create();

        strategy.MaxHedgedAttempts.Should().Be(_options.MaxHedgedAttempts);
        strategy.HedgingDelay.Should().Be(_options.HedgingDelay);
        strategy.HedgingDelayGenerator.Should().BeNull();
        strategy.HedgingHandler.Should().BeNull();
        strategy.HedgingHandler.Should().BeNull();
    }

    [Fact]
    public void Execute_Skipped_Ok()
    {
        var strategy = Create();

        strategy.Execute(_ => 10).Should().Be(10);
    }

    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [Theory]
    public async Task GetHedgingDelayAsync_GeneratorSet_EnsureCorrectGeneratedValue(int seconds)
    {
        _options.HedgingDelayGenerator.SetGenerator(args => TimeSpan.FromSeconds(seconds));

        var strategy = Create();

        var result = await strategy.GetHedgingDelayAsync(ResilienceContext.Get(), 0);

        result.Should().Be(TimeSpan.FromSeconds(seconds));
    }

    [Fact]
    public async Task GetHedgingDelayAsync_NoGeneratorSet_EnsureCorrectValue()
    {
        _options.HedgingDelay = TimeSpan.FromMilliseconds(123);

        var strategy = Create();

        var result = await strategy.GetHedgingDelayAsync(ResilienceContext.Get(), 0);

        result.Should().Be(TimeSpan.FromMilliseconds(123));
    }

    private HedgingResilienceStrategy Create() => new(_options);
}
