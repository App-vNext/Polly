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

        options.ShouldHandle.ShouldNotBeNull();

        options.DelayGenerator.ShouldBeNull();

        options.OnRetry.ShouldBeNull();

        options.MaxRetryAttempts.ShouldBe(3);
        options.BackoffType.ShouldBe(DelayBackoffType.Constant);
        options.Delay.ShouldBe(TimeSpan.FromSeconds(2));
        options.Name.ShouldBe("Retry");
        options.Randomizer.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new RetryStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromResult(0), 0))).ShouldBe(false);
        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException()), 0))).ShouldBe(false);
        (await options.ShouldHandle(new RetryPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException()), 0))).ShouldBe(true);
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
            Delay = TimeSpan.MinValue,
            MaxDelay = TimeSpan.FromSeconds(-10)
        };

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(options, "Invalid Options")));
        exception.Message.Trim().ShouldBe("""
            Invalid Options
            Validation Errors:
            The field MaxRetryAttempts must be between 0 and 2147483647.
            The field Delay must be between 00:00:00 and 1.00:00:00.
            The field MaxDelay must be between 00:00:00 and 1.00:00:00.
            The ShouldHandle field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
