namespace Polly.Specs.RateLimit;

public class RateLimitRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        const string Dummy = "dummy";
        var exception = new InvalidOperationException();
        var retryAfter = TimeSpan.FromSeconds(4);

        new RateLimitRejectedException().Message.ShouldBe("The operation could not be executed because it was rejected by the rate limit.");
        new RateLimitRejectedException(Dummy).Message.ShouldBe(Dummy);

        var rate = new RateLimitRejectedException(Dummy, exception);
        rate.Message.ShouldBe(Dummy);
        rate.InnerException.ShouldBe(exception);

        new RateLimitRejectedException(retryAfter).RetryAfter.ShouldBe(retryAfter);
        new RateLimitRejectedException(retryAfter).Message.ShouldBe($"The operation has been rate-limited and should be retried after {retryAfter}");

        rate = new RateLimitRejectedException(retryAfter, exception);
        rate.RetryAfter.ShouldBe(retryAfter);
        rate.InnerException.ShouldBe(exception);

        rate = new RateLimitRejectedException(retryAfter, Dummy);
        rate.RetryAfter.ShouldBe(retryAfter);
        rate.Message.ShouldBe(Dummy);

        rate = new RateLimitRejectedException(retryAfter, Dummy, exception);
        rate.RetryAfter.ShouldBe(retryAfter);
        rate.Message.ShouldBe(Dummy);
        rate.InnerException.ShouldBe(exception);
    }
}
