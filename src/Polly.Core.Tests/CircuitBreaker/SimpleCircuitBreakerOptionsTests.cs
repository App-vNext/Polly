using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.Strategy;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.CircuitBreaker;

public class SimpleCircuitBreakerOptionsTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        var options = new SimpleCircuitBreakerStrategyOptions();

        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureThreshold.Should().Be(100);
        options.OnOpened.Should().BeNull();
        options.OnClosed.Should().BeNull();
        options.OnHalfOpened.Should().BeNull();
        options.ShouldHandle.Should().BeNull();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeEmpty();

        // now set to min values
        options.FailureThreshold = 1;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);

        options.ShouldHandle = (_, _) => PredicateResult.True;
        ValidationHelper.ValidateObject(options, "Dummy.");
    }

    [Fact]
    public void Ctor_Generic_Defaults()
    {
        var options = new SimpleCircuitBreakerStrategyOptions<int>();

        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureThreshold.Should().Be(100);
        options.OnOpened.Should().BeNull();
        options.OnClosed.Should().BeNull();
        options.OnHalfOpened.Should().BeNull();
        options.ShouldHandle.Should().BeNull();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeEmpty();

        // now set to min values
        options.FailureThreshold = 1;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);

        options.ShouldHandle = (_, _) => PredicateResult.True;
        ValidationHelper.ValidateObject(options, "Dummy.");
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        bool onOpenedCalled = false;
        bool onClosedCalled = false;
        bool onHalfOpenCalled = false;

        var options = new SimpleCircuitBreakerStrategyOptions<int>
        {
            BreakDuration = TimeSpan.FromSeconds(123),
            FailureThreshold = 23,
            StrategyName = "dummy-name",
            OnOpened = (_, _) => { onOpenedCalled = true; return default; },
            OnClosed = (_, _) => { onClosedCalled = true; return default; },
            OnHalfOpened = (_) => { onHalfOpenCalled = true; return default; },
            ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException),
            ManualControl = new CircuitBreakerManualControl(),
            StateProvider = new CircuitBreakerStateProvider()
        };

        var converted = options.AsNonGenericOptions();

        // assert converted options
        converted.StrategyType.Should().Be("CircuitBreaker");
        converted.StrategyName.Should().Be("dummy-name");
        converted.FailureThreshold.Should().Be(23);
        converted.BreakDuration.Should().Be(TimeSpan.FromSeconds(123));
        converted.ManualControl.Should().Be(options.ManualControl);
        converted.StateProvider.Should().Be(options.StateProvider);

        var context = ResilienceContext.Get().Initialize<string>(false);

        // check other type
        (await converted.ShouldHandle!.Invoke(new Outcome(new InvalidOperationException()), new CircuitBreakerPredicateArguments(context))).Should().BeFalse();
        await converted.OnClosed!.Invoke(new Outcome(new InvalidOperationException()), new OnCircuitClosedArguments(context, true));
        await converted.OnOpened!.Invoke(new Outcome(new InvalidOperationException()), new OnCircuitOpenedArguments(context, TimeSpan.Zero, true));
        await converted.OnHalfOpened!.Invoke(new OnCircuitHalfOpenedArguments(context));
        onOpenedCalled.Should().BeFalse();
        onClosedCalled.Should().BeFalse();
        onHalfOpenCalled.Should().BeTrue();
        onHalfOpenCalled = false;

        // check correct type
        context = ResilienceContext.Get().Initialize<int>(false);
        (await converted.ShouldHandle!.Invoke(new Outcome(new InvalidOperationException()), new CircuitBreakerPredicateArguments(context))).Should().BeTrue();
        await converted.OnClosed!.Invoke(new Outcome(new InvalidOperationException()), new OnCircuitClosedArguments(context, true));
        await converted.OnOpened!.Invoke(new Outcome(new InvalidOperationException()), new OnCircuitOpenedArguments(context, TimeSpan.Zero, true));
        await converted.OnHalfOpened!.Invoke(new OnCircuitHalfOpenedArguments(context));
        onOpenedCalled.Should().BeTrue();
        onClosedCalled.Should().BeTrue();
        onHalfOpenCalled.Should().BeTrue();
    }

    [Fact]
    public void InvalidOptions_Validate()
    {
        var options = new SimpleCircuitBreakerStrategyOptions<int>
        {
            BreakDuration = TimeSpan.FromMilliseconds(299),
            FailureThreshold = 0,
            OnOpened = null!,
            OnClosed = null!,
            OnHalfOpened = null!,
            ShouldHandle = null!,
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Dummy."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Dummy.

            Validation Errors:
            The field FailureThreshold must be between 1 and 2147483647.
            The field BreakDuration must be >= to 00:00:00.5000000.
            The ShouldHandle field is required.
            """);
    }
}
