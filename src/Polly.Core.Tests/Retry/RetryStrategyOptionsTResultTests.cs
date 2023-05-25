using System.ComponentModel.DataAnnotations;
using Polly.Retry;
using Polly.Strategy;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryStrategyOptionsTResultTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var options = new RetryStrategyOptions<int>();

        options.StrategyType.Should().Be("Retry");
        options.ShouldRetry.Should().BeNull();

        options.RetryDelayGenerator.Should().BeNull();

        options.OnRetry.Should().BeNull();

        options.RetryCount.Should().Be(3);
        options.BackoffType.Should().Be(RetryBackoffType.ExponentialWithJitter);
        options.BaseDelay.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void InvalidOptions()
    {
        var options = new RetryStrategyOptions<int>
        {
            ShouldRetry = null!,
            RetryDelayGenerator = null!,
            OnRetry = null!,
            RetryCount = -3,
            BaseDelay = TimeSpan.MinValue
        };

        options.Invoking(o => ValidationHelper.ValidateObject(o, "Invalid Options"))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The field RetryCount must be between -1 and 100.
            The field BaseDelay must be >= to 00:00:00.
            The ShouldRetry field is required.
            """);
    }

    [Fact]
    public async Task AsNonGenericOptions_Ok()
    {
        var called = false;
        var options = new RetryStrategyOptions<int>
        {
            BackoffType = RetryBackoffType.Constant,
            BaseDelay = TimeSpan.FromMilliseconds(555),
            RetryCount = 7,
            StrategyName = "my-name",
            ShouldRetry = (outcome, _) => new ValueTask<bool>(outcome.HasResult && outcome.Result == 999),
            OnRetry = (_, _) => { called = true; return default; },
            RetryDelayGenerator = (_, _) => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123))
        };

        var nonGenericOptions = options.AsNonGenericOptions();

        nonGenericOptions.BackoffType.Should().Be(RetryBackoffType.Constant);
        nonGenericOptions.BaseDelay.Should().Be(TimeSpan.FromMilliseconds(555));
        nonGenericOptions.RetryCount.Should().Be(7);
        nonGenericOptions.StrategyName.Should().Be("my-name");
        nonGenericOptions.StrategyType.Should().Be("Retry");

        // wrong result type
        var context = ResilienceContext.Get().Initialize<string>(true);
        var args = new ShouldRetryArguments(context, 0);
        var delayArgs = new RetryDelayArguments(context, 2, TimeSpan.FromMinutes(1));
        var retryArgs = new OnRetryArguments(context, 0, TimeSpan.Zero);
        (await nonGenericOptions.ShouldRetry!(new Outcome(999), args)).Should().BeFalse();
        await nonGenericOptions.OnRetry!(new Outcome(999), retryArgs);
        (await nonGenericOptions.RetryDelayGenerator!(new Outcome(999), delayArgs)).Should().Be(TimeSpan.MinValue);
        called.Should().BeFalse();

        // correct result type
        context.Initialize<int>(true);
        (await nonGenericOptions.ShouldRetry!(new Outcome(999), args)).Should().BeTrue();
        await nonGenericOptions.OnRetry!(new Outcome(999), retryArgs);
        (await nonGenericOptions.RetryDelayGenerator!(new Outcome(999), delayArgs)).Should().Be(TimeSpan.FromSeconds(123));
        called.Should().BeTrue();
    }

    [Fact]
    public void AsNonGenericOptions_EmptyDelegates_Ok()
    {
        var options = new RetryStrategyOptions<int>().AsNonGenericOptions();

        options.ShouldRetry.Should().BeNull();
        options.RetryDelayGenerator.Should().BeNull();
        options.OnRetry.Should().BeNull();
    }
}
