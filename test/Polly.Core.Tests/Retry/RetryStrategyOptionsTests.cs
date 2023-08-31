using System.ComponentModel.DataAnnotations;
using Polly.Retry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var options = new RetryStrategyOptions<int>();

        options.ShouldHandle.Should().NotBeNull();

        options.DelayGenerator.Should().BeNull();

        options.OnRetry.Should().BeNull();

        options.MaxRetryAttempts.Should().Be(3);
        options.BackoffType.Should().Be(DelayBackoffType.Constant);
        options.Delay.Should().Be(TimeSpan.FromSeconds(2));
        options.Name.Should().Be("Retry");
        options.Randomizer.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new RetryStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromResult(0), 0))).Should().Be(false);
        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException()), 0))).Should().Be(false);
        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException()), 0))).Should().Be(true);
    }

    [Fact]
    public void InvalidOptions()
    {
        var options = new RetryStrategyOptions<int>
        {
            ShouldHandle = null!,
            DelayGenerator = null!,
            OnRetry = null!,
            MaxRetryAttempts = -3,
            Delay = TimeSpan.MinValue
        };

        options.Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid Options")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The field MaxRetryAttempts must be between 1 and 2147483647.
            The field Delay must be between 00:00:00 and 1.00:00:00.
            The ShouldHandle field is required.
            """);
    }
}
