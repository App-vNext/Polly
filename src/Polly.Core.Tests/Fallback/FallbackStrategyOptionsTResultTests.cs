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
        options.ShouldHandle.Should().BeNull();
        options.OnFallback.Should().BeNull();
        options.FallbackAction.Should().BeNull();
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions<int>
        {
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
            """);
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        var called = false;
        var options = new FallbackStrategyOptions<int>
        {
            OnFallback = (_, _) => { called = true; return default; },
            ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Result == -1),
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

        nonGeneric.OnFallback.Should().NotBeNull();
        await nonGeneric.OnFallback!(new Outcome(0), new OnFallbackArguments(ResilienceContext.Get()));
        called.Should().BeFalse();

        await nonGeneric.OnFallback!(new Outcome(0), new OnFallbackArguments(ResilienceContext.Get().Initialize<int>(true)));
        called.Should().BeTrue();
    }
}
