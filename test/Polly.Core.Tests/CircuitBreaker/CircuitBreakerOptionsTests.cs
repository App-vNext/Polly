using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerOptionsTests
{
    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new CircuitBreakerStrategyOptions();
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        (await options.ShouldHandle(new CircuitBreakerPredicateArguments<object>(context, Outcome.FromResult<object>("dummy")))).ShouldBe(false);
        (await options.ShouldHandle(new CircuitBreakerPredicateArguments<object>(context, Outcome.FromException<object>(new OperationCanceledException())))).ShouldBe(false);
        (await options.ShouldHandle(new CircuitBreakerPredicateArguments<object>(context, Outcome.FromException<object>(new InvalidOperationException())))).ShouldBe(true);
    }

    [Fact]
    public void Ctor_Defaults()
    {
        var options = new CircuitBreakerStrategyOptions<int>();

        options.BreakDuration.ShouldBe(TimeSpan.FromSeconds(5));
        options.FailureRatio.ShouldBe(0.1);
        options.MinimumThroughput.ShouldBe(100);
        options.SamplingDuration.ShouldBe(TimeSpan.FromSeconds(30));
        options.OnOpened.ShouldBeNull();
        options.OnClosed.ShouldBeNull();
        options.OnHalfOpened.ShouldBeNull();
        options.ShouldHandle.ShouldNotBeNull();
        options.Name.ShouldBe("CircuitBreaker");

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

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(options, "Dummy.")));
        exception.Message.Trim().ShouldBe("""
            Dummy.
            Validation Errors:
            The field MinimumThroughput must be between 2 and 2147483647.
            The field SamplingDuration must be between 00:00:00.5000000 and 1.00:00:00.
            The field BreakDuration must be between 00:00:00.5000000 and 1.00:00:00.
            The ShouldHandle field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
