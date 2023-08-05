using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerOptionsTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        var options = new CircuitBreakerStrategyOptions();
        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureRatio.Should().Be(0.1);
        options.MinimumThroughput.Should().Be(100);
        options.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.OnOpened.Should().BeNull();
        options.OnClosed.Should().BeNull();
        options.OnHalfOpened.Should().BeNull();
        options.ShouldHandle.Should().NotBeNull();
        options.Name.Should().BeNull();

        // now set to min values
        options.FailureRatio = 0.001;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromMilliseconds(500);

        ValidationHelper.ValidateObject(new(options, "Dummy."));
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new CircuitBreakerStrategyOptions();
        var args = default(CircuitBreakerPredicateArguments);
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new(context, Outcome.FromResult<object>("dummy"), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<object>(new OperationCanceledException()), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<object>(new InvalidOperationException()), args))).Should().Be(true);
    }

    [Fact]
    public void Ctor_Generic_Defaults()
    {
        var options = new CircuitBreakerStrategyOptions<int>();

        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.FailureRatio.Should().Be(0.1);
        options.MinimumThroughput.Should().Be(100);
        options.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.OnOpened.Should().BeNull();
        options.OnClosed.Should().BeNull();
        options.OnHalfOpened.Should().BeNull();
        options.ShouldHandle.Should().NotBeNull();
        options.Name.Should().BeNull();

        // now set to min values
        options.FailureRatio = 0.001;
        options.BreakDuration = TimeSpan.FromMilliseconds(500);
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromMilliseconds(500);

        ValidationHelper.ValidateObject(new(options, "Dummy."));
    }

    [Fact]
    public void InvalidOptions_Validate()
    {
        var options = new CircuitBreakerStrategyOptions<int>
        {
            BreakDuration = TimeSpan.FromMilliseconds(299),
            FailureRatio = 0,
            SamplingDuration = TimeSpan.Zero,
            MinimumThroughput = 0,
            OnOpened = null!,
            OnClosed = null!,
            OnHalfOpened = null!,
            ShouldHandle = null!,
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Dummy.")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Dummy.

            Validation Errors:
            The field MinimumThroughput must be between 2 and 2147483647.
            The field SamplingDuration must be between 00:00:00.5000000 and 1.00:00:00.
            The field BreakDuration must be between 00:00:00.5000000 and 1.00:00:00.
            The ShouldHandle field is required.
            """);
    }
}
