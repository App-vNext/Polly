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
        options.ShouldHandle.Should().NotBeNull();

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
        var args = new RetryPredicateArguments(0);
        var context = ResilienceContext.Get();

        (await options.ShouldHandle(new(context, Outcome.FromResult(0), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<int>(new OperationCanceledException()), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<int>(new InvalidOperationException()), args))).Should().Be(true);
    }

    [Fact]
    public void InvalidOptions()
    {
        var options = new RetryStrategyOptions<int>
        {
            ShouldHandle = null!,
            RetryDelayGenerator = null!,
            OnRetry = null!,
            RetryCount = -3,
            BaseDelay = TimeSpan.MinValue
        };

        options.Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid Options")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The field RetryCount must be between 1 and 2147483647.
            The field BaseDelay must be between 00:00:00 and 1.00:00:00.
            The ShouldHandle field is required.
            """);
    }
}
