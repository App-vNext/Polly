﻿namespace Polly.Core.Tests.Simmy;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

public class MonkeyStrategyTests
{
    private readonly TestChaosStrategyOptions _options;

    public MonkeyStrategyTests() => _options = new();

    public static List<object[]> CtorTestCases =>
        new()
        {
            new object[] { null, "Value cannot be null. (Parameter 'options')" },
            new object[] { new TestChaosStrategyOptions(), "Either InjectionRate or InjectionRateGenerator is required. (Parameter 'InjectionRate')" },
            new object[] { new TestChaosStrategyOptions { InjectionRate = 0.5 }, "Either Enabled or EnabledGenerator is required. (Parameter 'Enabled')" }
        };

    [Theory]
    [MemberData(nameof(CtorTestCases))]
    public void InvalidCtor(TestChaosStrategyOptions options, string message)
    {
        Action act = () =>
        {
            var _ = new TestChaosStrategy(options);
        };

        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage(message);
    }

    [Fact]
    public async Task Ctor_Ok()
    {
        var context = ResilienceContext.Get();
        _options.EnabledGenerator = (_) => new ValueTask<bool>(true);
        _options.InjectionRate = 0.5;

        var sut = CreateSut();

        sut.EnabledGenerator.Should().NotBeNull();
        (await sut.EnabledGenerator(context)).Should().BeTrue();

        sut.InjectionRateGenerator.Should().NotBeNull();
        (await sut.InjectionRateGenerator(context)).Should().Be(0.5);
    }

    [InlineData(-1)]
    [InlineData(1.1)]
    [Theory]
    public async Task Should_throw_error_when_injection_rate_generator_result_is_not_valid(double injectionRate)
    {
        var wasMonkeyUnleashed = false;

        _options.EnabledGenerator = (_) => new ValueTask<bool>(true);
        _options.InjectionRateGenerator = (_) => new ValueTask<double>(injectionRate);
        _options.Randomizer = () => 0.5;

        var sut = CreateSut();

        await sut.Invoking(s => s.ExecuteAsync((_) => { return default; }).AsTask())
            .Should()
            .ThrowAsync<ArgumentOutOfRangeException>();

        wasMonkeyUnleashed.Should().BeFalse();
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

        await sut.ExecuteAsync((_) => { return default; });

        wasMonkeyUnleashed.Should().BeTrue();
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

        await sut.Invoking(s => s.ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        wasMonkeyUnleashed.Should().BeFalse();
        enableGeneratorExecuted.Should().BeFalse();
        injectionRateGeneratorExecuted.Should().BeFalse();
    }

    private TestChaosStrategy CreateSut() => new(_options);
}
