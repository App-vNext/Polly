using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.Utils;

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
        options.ShouldHandle.Should().NotBeNull();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeNull();

        // now set to min values
        options.FailureThreshold = 1;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);

        ValidationHelper.ValidateObject(options, "Dummy.");
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new SimpleCircuitBreakerStrategyOptions();
        var args = new CircuitBreakerPredicateArguments();
        var context = ResilienceContext.Get();

        (await options.ShouldHandle(new(context, new Outcome<int>(0), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, new Outcome<int>(new OperationCanceledException()), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, new Outcome<int>(new InvalidOperationException()), args))).Should().Be(true);
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
        options.ShouldHandle.Should().NotBeNull();
        options.StrategyType.Should().Be("CircuitBreaker");
        options.StrategyName.Should().BeNull();

        // now set to min values
        options.FailureThreshold = 1;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);

        options.ShouldHandle = _ => PredicateResult.True;
        ValidationHelper.ValidateObject(options, "Dummy.");
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
