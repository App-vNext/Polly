using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Strategy;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingStrategyOptionsTResultTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions<int>();

        options.StrategyType.Should().Be("Hedging");
        options.ShouldHandle.Should().BeNull();
        options.HedgingActionGenerator.Should().BeNull();
        options.HedgingDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.Should().Be(2);
        options.OnHedging.Should().BeNull();
    }

    [Fact]
    public void Validation()
    {
        var options = new HedgingStrategyOptions<int>
        {
            HedgingDelayGenerator = null!,
            ShouldHandle = null!,
            MaxHedgedAttempts = -1,
            OnHedging = null!,
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The field MaxHedgedAttempts must be between 2 and 10.
            The ShouldHandle field is required.
            The HedgingActionGenerator field is required.
            """);
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        var onHedgingCalled = false;
        var options = new HedgingStrategyOptions<int>
        {
            HedgingDelayGenerator = args => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123)),
            ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Result == -1),
            StrategyName = "Dummy",
            HedgingDelay = TimeSpan.FromSeconds(3),
            MaxHedgedAttempts = 4,
            HedgingActionGenerator = args => () => Task.FromResult(555),
            OnHedging = (_, args) =>
            {
                args.Context.Should().NotBeNull();
                args.Attempt.Should().Be(3);
                onHedgingCalled = true;
                return default;
            }
        };

        var nonGeneric = options.AsNonGenericOptions();

        nonGeneric.Should().NotBeNull();
        nonGeneric.StrategyType.Should().Be("Hedging");
        nonGeneric.StrategyName.Should().Be("Dummy");
        nonGeneric.Handler.IsEmpty.Should().BeFalse();
        nonGeneric.MaxHedgedAttempts.Should().Be(4);
        nonGeneric.HedgingDelay.Should().Be(TimeSpan.FromSeconds(3));

        var handler = nonGeneric.Handler.CreateHandler();
        handler.Should().NotBeNull();

        (await handler!.TryCreateHedgedAction<int>(ResilienceContext.Get(), 0)!()).Should().Be(555);

        var result = await handler!.ShouldHandleAsync(new Outcome<int>(-1), new HandleHedgingArguments(ResilienceContext.Get()));
        result.Should().BeTrue();

        result = await handler!.ShouldHandleAsync(new Outcome<int>(0), new HandleHedgingArguments(ResilienceContext.Get()));
        result.Should().BeFalse();

        var delay = await nonGeneric.HedgingDelayGenerator!(new HedgingDelayArguments(ResilienceContext.Get(), 4));
        delay.Should().Be(TimeSpan.FromSeconds(123));

        await nonGeneric.OnHedging!(new Outcome(10), new OnHedgingArguments(ResilienceContext.Get(), 3));
        onHedgingCalled.Should().BeFalse();

        await nonGeneric.OnHedging!(new Outcome(10), new OnHedgingArguments(ResilienceContext.Get().Initialize<int>(true), 3));
        onHedgingCalled.Should().BeTrue();
    }
}
