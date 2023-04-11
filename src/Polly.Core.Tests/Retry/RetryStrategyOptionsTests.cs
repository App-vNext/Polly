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

        options.ShouldRetry.Should().NotBeNull();
        options.ShouldRetry.IsEmpty.Should().BeTrue();

        options.RetryDelayGenerator.Should().NotBeNull();
        options.RetryDelayGenerator.IsEmpty.Should().BeTrue();

        options.OnRetry.Should().NotBeNull();
        options.OnRetry.IsEmpty.Should().BeTrue();

        options.RetryCount.Should().Be(3);
        options.BackoffType.Should().Be(RetryBackoffType.Exponential);
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
            The RetryDelayGenerator field is required.
            The OnRetry field is required.
            """);
    }
}
