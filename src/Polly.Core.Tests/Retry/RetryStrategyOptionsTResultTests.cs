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

        options.ShouldRetry.Should().NotBeNull();
        options.ShouldRetry.IsEmpty.Should().BeTrue();

        options.RetryDelayGenerator.Should().NotBeNull();
        options.RetryDelayGenerator.IsEmpty.Should().BeTrue();

        options.OnRetry.Should().NotBeNull();
        options.OnRetry.IsEmpty.Should().BeTrue();

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
            The RetryDelayGenerator field is required.
            The OnRetry field is required.
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
        };

        options.ShouldRetry.HandleResult(999);
        options.OnRetry.Register(() => called = true);
        options.RetryDelayGenerator.SetGenerator((_, _) => TimeSpan.FromSeconds(123));
        var nonGenericOptions = options.AsNonGenericOptions();

        nonGenericOptions.BackoffType.Should().Be(RetryBackoffType.Constant);
        nonGenericOptions.BaseDelay.Should().Be(TimeSpan.FromMilliseconds(555));
        nonGenericOptions.RetryCount.Should().Be(7);
        nonGenericOptions.StrategyName.Should().Be("my-name");
        nonGenericOptions.StrategyType.Should().Be("Retry");

        var args = new ShouldRetryArguments(ResilienceContext.Get(), 0);
        var delayArgs = new RetryDelayArguments(ResilienceContext.Get(), 2, TimeSpan.FromMinutes(1));
        var retryArgs = new OnRetryArguments(ResilienceContext.Get(), 0, TimeSpan.Zero);

        (await nonGenericOptions.ShouldRetry.CreateHandler()!.ShouldHandleAsync(new Outcome<int>(999), args)).Should().BeTrue();
        await nonGenericOptions.OnRetry.CreateHandler()!.HandleAsync(new Outcome<int>(999), retryArgs);
        called.Should().BeTrue();
        (await nonGenericOptions.RetryDelayGenerator.CreateHandler(default, _ => true)!.GenerateAsync(new Outcome<int>(999), delayArgs)).Should().Be(TimeSpan.FromSeconds(123));
    }
}
