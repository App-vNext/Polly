using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Strategy;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

public class FallbackStrategyOptionsTResultTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions<int>();

        options.StrategyType.Should().Be("Fallback");
        options.ShouldHandle.Should().NotBeNull();
        options.ShouldHandle.IsEmpty.Should().BeTrue();
        options.OnFallback.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions<int>
        {
            OnFallback = null!,
            ShouldHandle = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            The OnFallback field is required.
            """);
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        var called = false;
        var options = new FallbackStrategyOptions<int>
        {
            OnFallback = new OutcomeEvent<OnFallbackArguments, int>().Register(() => called = true),
            ShouldHandle = new OutcomePredicate<HandleFallbackArguments, int>().HandleResult(-1),
            FallbackAction = (_, _) => new ValueTask<int>(1),
            StrategyName = "Dummy",
        };

        var nonGeneric = options.AsNonGenericOptions();

        nonGeneric.Should().NotBeNull();
        nonGeneric.StrategyType.Should().Be("Fallback");
        nonGeneric.StrategyName.Should().Be("Dummy");
        nonGeneric.Handler.IsEmpty.Should().BeFalse();

        var handler = nonGeneric.Handler.CreateHandler();
        handler.Should().NotBeNull();

        var result = await handler!.ShouldHandleAsync(new Outcome<int>(-1), new HandleFallbackArguments(ResilienceContext.Get()));
        result.Should().Be(options.FallbackAction);

        result = await handler!.ShouldHandleAsync(new Outcome<int>(0), new HandleFallbackArguments(ResilienceContext.Get()));
        result.Should().BeNull();

        nonGeneric.OnFallback.IsEmpty.Should().BeFalse();
        await nonGeneric.OnFallback.CreateHandler()!.HandleAsync(new Outcome<int>(0), new OnFallbackArguments(ResilienceContext.Get()));

        called.Should().BeTrue();
    }
}
