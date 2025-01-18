using Polly.RateLimiting;

namespace Polly.Core.Tests.Timeout;

public class RateLimiterRejectedExceptionTests
{
    private readonly string _message = "dummy";
    private readonly TimeSpan _retryAfter = TimeSpan.FromSeconds(4);

    [Fact]
    public void Ctor_Ok()
    {
        var exception = new RateLimiterRejectedException();
        exception.InnerException.ShouldBeNull();
        exception.Message.ShouldBe("The operation could not be executed because it was rejected by the rate limiter.");
        exception.RetryAfter.ShouldBeNull();
        exception.TelemetrySource.ShouldBeNull();
    }

    [Fact]
    public void Ctor_RetryAfter_Ok()
    {
        var exception = new RateLimiterRejectedException(_retryAfter);
        exception.InnerException.ShouldBeNull();
        exception.Message.ShouldBe($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '00:00:04'.");
        exception.RetryAfter.ShouldBe(_retryAfter);
        exception.TelemetrySource.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_Ok()
    {
        var exception = new RateLimiterRejectedException(_message);
        exception.InnerException.ShouldBeNull();
        exception.Message.ShouldBe(_message);
        exception.RetryAfter.ShouldBeNull();
        exception.TelemetrySource.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, _retryAfter);
        exception.InnerException.ShouldBeNull();
        exception.Message.ShouldBe(_message);
        exception.RetryAfter.ShouldBe(_retryAfter);
        exception.TelemetrySource.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_InnerException_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, new InvalidOperationException());
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe(_message);
        exception.RetryAfter.ShouldBeNull();
        exception.TelemetrySource.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_InnerException_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, _retryAfter, new InvalidOperationException());
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe(_message);
        exception.RetryAfter.ShouldBe(_retryAfter);
        exception.TelemetrySource.ShouldBeNull();
    }

#if NETFRAMEWORK
    [Fact]
    public void BinaryDeserialization_Ok()
    {
        var timeout = TimeSpan.FromSeconds(4);
        var result = SerializeAndDeserializeException(new RateLimiterRejectedException(timeout));
        result.RetryAfter.ShouldBe(timeout);

        result = SerializeAndDeserializeException(new RateLimiterRejectedException());
        result.RetryAfter.ShouldBeNull();
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
