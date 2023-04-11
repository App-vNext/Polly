using Polly.RateLimiting;

namespace Polly.Core.Tests.Timeout;

public class RateLimiterRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var retryAfter = TimeSpan.FromSeconds(4);

        new RateLimiterRejectedException().Message.Should().Be("The operation could not be executed because it was rejected by the rate limiter.");
        new RateLimiterRejectedException().RetryAfter.Should().BeNull();
        new RateLimiterRejectedException("dummy").Message.Should().Be("dummy");
        new RateLimiterRejectedException("dummy", new InvalidOperationException()).Message.Should().Be("dummy");
        new RateLimiterRejectedException(retryAfter).RetryAfter.Should().Be(retryAfter);
        new RateLimiterRejectedException(retryAfter).Message.Should().Be($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '{retryAfter}'.");
        new RateLimiterRejectedException("dummy", retryAfter).RetryAfter.Should().Be(retryAfter);
        new RateLimiterRejectedException("dummy", retryAfter, new InvalidOperationException()).RetryAfter.Should().Be(retryAfter);
    }

#if !NETCOREAPP
    [Fact]
    public void BinaryDeserialization_Ok()
    {
        var timeout = TimeSpan.FromSeconds(4);
        var result = SerializeAndDeserializeException(new RateLimiterRejectedException(timeout));
        result.RetryAfter.Should().Be(timeout);

        result = SerializeAndDeserializeException(new RateLimiterRejectedException());
        result.RetryAfter.Should().BeNull();
    }

    public static T SerializeAndDeserializeException<T>(T exception)
        where T : Exception
    {
        using var stream = new MemoryStream();
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        formatter.Serialize(stream, exception);
        stream.Position = 0;
        return (T)formatter.Deserialize(stream);
    }
#endif
}
