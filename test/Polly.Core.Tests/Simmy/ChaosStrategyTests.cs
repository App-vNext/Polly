namespace Polly.Core.Tests.Simmy;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

public class ChaosStrategyTests
{
    private readonly TestChaosStrategyOptions _options;

    public ChaosStrategyTests() => _options = new();

    [Fact]
    public void InvalidCtor()
    {
        Action act = () =>
        {
            var _ = new TestChaosStrategy(null);
        };

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Ctor_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        _options.EnabledGenerator = (_) => new ValueTask<bool>(true);
        _options.InjectionRate = 0.5;

        var sut = CreateSut();

        sut.EnabledGenerator.Should().NotBeNull();
        (await sut.EnabledGenerator(new(context))).Should().BeTrue();

        sut.InjectionRateGenerator.Should().NotBeNull();
        (await sut.InjectionRateGenerator(new(context))).Should().Be(0.5);
    }

    [InlineData(0, false)]
    [InlineData(0.5, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(1.1, true)]
    [Theory]
    public async Task Should_coerce_injection_rate_generator_result_is_not_valid(double injectionRateGeneratorResult, bool shouldBeInjected)
    {
        var wasMonkeyUnleashed = false;

        _options.EnabledGenerator = (_) => new ValueTask<bool>(true);
        _options.InjectionRateGenerator = (_) => new ValueTask<double>(injectionRateGeneratorResult);
        _options.Randomizer = () => 0.5;

        var sut = CreateSut();
        sut.OnExecute = (_, _) => { wasMonkeyUnleashed = true; return Task.CompletedTask; };

        await sut.AsPipeline().ExecuteAsync((_) => { return default; });

        wasMonkeyUnleashed.Should().Be(shouldBeInjected);
    }

    [Fact]
    public async Task Should_not_inject_chaos_when_it_was_cancelled_before_evaluating_strategy()
    {
        var wasMonkeyUnleashed = false;
        var enableGeneratorExecuted = false;
        var injectionRateGeneratorExecuted = false;

        _options.Randomizer = () => 0.5;
        _options.EnabledGenerator = (_) =>
        {
            enableGeneratorExecuted = true;
            return new ValueTask<bool>(true);
        };
        _options.InjectionRateGenerator = (_) =>
        {
            injectionRateGeneratorExecuted = true;
            return new ValueTask<double>(0.6);
        };

        using var cts = new CancellationTokenSource();
        var sut = CreateSut();
        sut.Before = (_, _) => { cts.Cancel(); };

        await sut.Invoking(s => s.AsPipeline().ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        wasMonkeyUnleashed.Should().BeFalse();
        enableGeneratorExecuted.Should().BeFalse();
        injectionRateGeneratorExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_inject_chaos_when_it_was_cancelled_on_enable_generator()
    {
        var wasMonkeyUnleashed = false;
        var enableGeneratorExecuted = false;
        var injectionRateGeneratorExecuted = false;

        using var cts = new CancellationTokenSource();
        _options.Randomizer = () => 0.5;
        _options.EnabledGenerator = (_) =>
        {
            cts.Cancel();
            enableGeneratorExecuted = true;
            return new ValueTask<bool>(true);
        };
        _options.InjectionRateGenerator = (_) =>
        {
            injectionRateGeneratorExecuted = true;
            return new ValueTask<double>(0.6);
        };

        var sut = CreateSut();

        await sut.Invoking(s => s.AsPipeline().ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        wasMonkeyUnleashed.Should().BeFalse();
        enableGeneratorExecuted.Should().BeTrue();
        injectionRateGeneratorExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_inject_chaos_when_it_was_cancelled_on_injection_rate_generator()
    {
        var wasMonkeyUnleashed = false;
        var enableGeneratorExecuted = false;
        var injectionRateGeneratorExecuted = false;

        using var cts = new CancellationTokenSource();
        _options.Randomizer = () => 0.5;
        _options.EnabledGenerator = (_) =>
        {
            enableGeneratorExecuted = true;
            return new ValueTask<bool>(true);
        };
        _options.InjectionRateGenerator = (_) =>
        {
            cts.Cancel();
            injectionRateGeneratorExecuted = true;
            return new ValueTask<double>(0.6);
        };

        var sut = CreateSut();

        await sut.Invoking(s => s.AsPipeline().ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        wasMonkeyUnleashed.Should().BeFalse();
        enableGeneratorExecuted.Should().BeTrue();
        injectionRateGeneratorExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_inject_chaos()
    {
        var wasMonkeyUnleashed = false;

        _options.EnabledGenerator = (_) => new ValueTask<bool>(true);
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;

        var sut = CreateSut();
        sut.OnExecute = (_, _) => { wasMonkeyUnleashed = true; return Task.CompletedTask; };

        await sut.AsPipeline().ExecuteAsync((_) => { return default; });

        wasMonkeyUnleashed.Should().BeTrue();
    }

    private TestChaosStrategy CreateSut() => new(_options);
}
