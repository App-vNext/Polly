using System.ComponentModel.DataAnnotations;
using Polly.Retry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var options = new RetryStrategyOptions();

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
        var options = new RetryStrategyOptions
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
