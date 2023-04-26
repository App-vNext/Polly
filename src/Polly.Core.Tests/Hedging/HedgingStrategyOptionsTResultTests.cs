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
        options.ShouldHandle.Should().NotBeNull();
        options.ShouldHandle.IsEmpty.Should().BeTrue();
        options.HedgingActionGenerator.Should().BeNull();
        options.HedgingDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.Should().Be(2);
        options.OnHedging.IsEmpty.Should().BeTrue();
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
            The HedgingDelayGenerator field is required.
            The OnHedging field is required.
            """);
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        var onHedgingCalled = false;
        var options = new HedgingStrategyOptions<int>
        {
            HedgingDelayGenerator = new NoOutcomeGenerator<HedgingDelayArguments, TimeSpan>().SetGenerator(args => TimeSpan.FromSeconds(123)),
            ShouldHandle = new OutcomePredicate<HandleHedgingArguments, int>().HandleResult(-1),
            StrategyName = "Dummy",
            StrategyType = "Dummy-Hedging",
            HedgingDelay = TimeSpan.FromSeconds(3),
            MaxHedgedAttempts = 4,
            HedgingActionGenerator = args => () => Task.FromResult(555),
            OnHedging = new OutcomeEvent<OnHedgingArguments, int>().Register((_, args) =>
            {
                args.Context.Should().NotBeNull();
                args.Attempt.Should().Be(3);
                onHedgingCalled = true;
            })
        };

        var nonGeneric = options.AsNonGenericOptions();

        nonGeneric.Should().NotBeNull();
        nonGeneric.StrategyType.Should().Be("Dummy-Hedging");
        nonGeneric.StrategyName.Should().Be("Dummy");
        nonGeneric.Handler.IsEmpty.Should().BeFalse();
        nonGeneric.MaxHedgedAttempts.Should().Be(4);
        nonGeneric.HedgingDelay.Should().Be(TimeSpan.FromSeconds(3));

        var handler = nonGeneric.Handler.CreateHandler();
        handler.Should().NotBeNull();

        (await handler!.TryCreateHedgedAction<int>(ResilienceContext.Get())!()).Should().Be(555);

        var result = await handler!.ShouldHandleAsync(new Outcome<int>(-1), new HandleHedgingArguments(ResilienceContext.Get()));
        result.Should().BeTrue();

        result = await handler!.ShouldHandleAsync(new Outcome<int>(0), new HandleHedgingArguments(ResilienceContext.Get()));
        result.Should().BeFalse();

        var delay = await nonGeneric.HedgingDelayGenerator.CreateHandler(TimeSpan.Zero, _ => true)!(new HedgingDelayArguments(ResilienceContext.Get(), 4));
        delay.Should().Be(TimeSpan.FromSeconds(123));

        await nonGeneric.OnHedging.CreateHandler()!.HandleAsync<int>(new Outcome<int>(10), new OnHedgingArguments(ResilienceContext.Get(), 3));
        onHedgingCalled.Should().BeTrue();
    }
}
