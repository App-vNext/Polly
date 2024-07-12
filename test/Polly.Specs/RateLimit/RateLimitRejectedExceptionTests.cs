namespace Polly.Specs.RateLimit;

public class RateLimitRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        const string Dummy = "dummy";
        var exception = new InvalidOperationException();
        var retryAfter = TimeSpan.FromSeconds(4);

        new RateLimitRejectedException().Message.Should().Be("The operation could not be executed because it was rejected by the rate limit.");
        new RateLimitRejectedException(Dummy).Message.Should().Be(Dummy);

        var rate = new RateLimitRejectedException(Dummy, exception);
        rate.Message.Should().Be(Dummy);
        rate.InnerException.Should().Be(exception);

        new RateLimitRejectedException(retryAfter).RetryAfter.Should().Be(retryAfter);
        new RateLimitRejectedException(retryAfter).Message.Should().Be($"The operation has been rate-limited and should be retried after {retryAfter}");

        rate = new RateLimitRejectedException(retryAfter, exception);
        rate.RetryAfter.Should().Be(retryAfter);
        rate.InnerException.Should().Be(exception);

        rate = new RateLimitRejectedException(retryAfter, Dummy);
        rate.RetryAfter.Should().Be(retryAfter);
        rate.Message.Should().Be(Dummy);

        rate = new RateLimitRejectedException(retryAfter, Dummy, exception);
        rate.RetryAfter.Should().Be(retryAfter);
        rate.Message.Should().Be(Dummy);
        rate.InnerException.Should().Be(exception);
    }
}
