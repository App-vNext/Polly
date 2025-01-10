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
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be("The operation could not be executed because it was rejected by the rate limiter.");
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().BeNull();
    }

    [Fact]
    public void Ctor_RetryAfter_Ok()
    {
        var exception = new RateLimiterRejectedException(_retryAfter);
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '00:00:04'.");
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_Ok()
    {
        var exception = new RateLimiterRejectedException(_message);
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be(_message);
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, _retryAfter);
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be(_message);
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_InnerException_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, new InvalidOperationException());
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be(_message);
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_InnerException_Ok()
    {
        var exception = new RateLimiterRejectedException(_message, _retryAfter, new InvalidOperationException());
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Be(_message);
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().BeNull();
    }

#if NETFRAMEWORK
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
