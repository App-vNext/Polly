using Polly.RateLimiting;
using Polly.Telemetry;
using Polly.TestUtils;

namespace Polly.Core.Tests.Timeout;

public class RateLimiterRejectedExceptionTests
{
    private readonly string _telemetrySource = "MyPipeline/MyPipelineInstance/MyRateLimiterStrategy";
    private readonly string _message = "dummy";
    private readonly TimeSpan _retryAfter = TimeSpan.FromSeconds(4);
    private readonly ResilienceTelemetrySource _source = new("MyPipeline", "MyPipelineInstance", "MyRateLimiterStrategy");

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
    public void Ctor_TelemetrySource_Ok()
    {
        var exception = new RateLimiterRejectedException(_source);
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be("The operation could not be executed because it was rejected by the rate limiter.");
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().Be(_telemetrySource);
    }

    [Fact]
    public void Ctor_TelemetrySource_Null_Ok()
    {
        // Arrange
        ResilienceTelemetrySource? source = null;

        // Act
        var exception = new RateLimiterRejectedException(source!);

        // Assert
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be("The operation could not be executed because it was rejected by the rate limiter.");
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().Be("(null)/(null)/(null)");
    }

    [Fact]
    public void Ctor_TelemetrySource_Nulls_Ok()
    {
        // Arrange
        var source = new ResilienceTelemetrySource(null, null, null);

        // Act
        var exception = new RateLimiterRejectedException(source);

        // Arrange
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be("The operation could not be executed because it was rejected by the rate limiter.");
        exception.RetryAfter.Should().BeNull();
        exception.TelemetrySource.Should().Be("(null)/(null)/(null)");
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
    public void Ctor_TelemetrySource_RetryAfter_Ok()
    {
        var exception = new RateLimiterRejectedException(_source, _retryAfter);
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '00:00:04'.");
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().Be(_telemetrySource);
    }

    [Fact]
    public void Ctor_TelemetrySource_Null_RetryAfter_Ok()
    {
        // Arrange
        ResilienceTelemetrySource? source = null;

        // Act
        var exception = new RateLimiterRejectedException(source!, _retryAfter);

        // Assert
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '00:00:04'.");
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().Be("(null)/(null)/(null)");
    }

    [Fact]
    public void Ctor_TelemetrySource_Nulls_RetryAfter_Ok()
    {
        // Arrange
        var source = new ResilienceTelemetrySource(null, null, null);

        // Act
        var exception = new RateLimiterRejectedException(source, _retryAfter);

        // Assert
        exception.InnerException.Should().BeNull();
        exception.Message.Should().Be($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '00:00:04'.");
        exception.RetryAfter.Should().Be(_retryAfter);
        exception.TelemetrySource.Should().Be("(null)/(null)/(null)");
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

#if !NETCOREAPP
    [Fact]
    public void BinaryDeserialization_Ok()
    {
        var timeout = TimeSpan.FromSeconds(4);
        var result = BinarySerializationUtil.SerializeAndDeserializeException(new RateLimiterRejectedException(timeout));
        result.RetryAfter.Should().Be(timeout);

        result = BinarySerializationUtil.SerializeAndDeserializeException(new RateLimiterRejectedException(TimeSpan.Zero));
        result.RetryAfter.Should().BeNull();

        result = BinarySerializationUtil.SerializeAndDeserializeException(new RateLimiterRejectedException());
        result.RetryAfter.Should().BeNull();

        result = BinarySerializationUtil.SerializeAndDeserializeException(new RateLimiterRejectedException(_source));
        result.TelemetrySource.Should().Be(_telemetrySource);
    }
#endif
}
