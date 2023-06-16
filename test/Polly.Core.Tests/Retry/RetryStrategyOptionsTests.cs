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

        options.StrategyType.Should().Be("Retry");
        options.ShouldRetry.Should().NotBeNull();

        options.RetryDelayGenerator.Should().BeNull();

        options.OnRetry.Should().BeNull();

        options.RetryCount.Should().Be(3);
        options.BackoffType.Should().Be(RetryBackoffType.Constant);
        options.BaseDelay.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new RetryStrategyOptions<int>();
        var args = new ShouldRetryArguments(0);
        var context = ResilienceContext.Get();

        (await options.ShouldRetry(new(context, new Outcome<int>(0), args))).Should().Be(false);
        (await options.ShouldRetry(new(context, new Outcome<int>(new OperationCanceledException()), args))).Should().Be(false);
        (await options.ShouldRetry(new(context, new Outcome<int>(new InvalidOperationException()), args))).Should().Be(true);
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
}
