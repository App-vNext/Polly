using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BrokenCircuitExceptionTests
{
    [Fact]
    public void Ctor_Default_Ok()
    {
        var exception = new BrokenCircuitException();
        exception.Message.Should().Be("The circuit is now open and is not allowing calls.");
        exception.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage);
        exception.Message.Should().Be(TestMessage);
        exception.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void Ctor_RetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestRetryAfter);
        exception.Message.Should().Be($"The circuit is now open and is not allowing calls. It can be retried after '{TestRetryAfter}'.");
        exception.RetryAfter.Should().Be(TestRetryAfter);
    }

    [Fact]
    public void Ctor_Message_RetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter);
        exception.Message.Should().Be(TestMessage);
        exception.RetryAfter.Should().Be(TestRetryAfter);
    }

    [Fact]
    public void Ctor_Message_InnerException_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, new InvalidOperationException());
        exception.Message.Should().Be(TestMessage);
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        exception.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_InnerException_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter, new InvalidOperationException());
        exception.Message.Should().Be(TestMessage);
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        exception.RetryAfter.Should().Be(TestRetryAfter);
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_NonNullRetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter, new InvalidOperationException());
        BrokenCircuitException roundtripResult = BinarySerializationUtil.SerializeAndDeserializeException(exception);
        roundtripResult.Should().NotBeNull();
        roundtripResult.Message.Should().Be(TestMessage);
        roundtripResult.InnerException.Should().BeOfType<InvalidOperationException>();
        roundtripResult.RetryAfter.Should().Be(TestRetryAfter);
    }

    [Fact]
    public void BinarySerialization_NullRetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, new InvalidOperationException());
        BrokenCircuitException roundtripResult = BinarySerializationUtil.SerializeAndDeserializeException(exception);
        roundtripResult.Should().NotBeNull();
        roundtripResult.Message.Should().Be(TestMessage);
        roundtripResult.InnerException.Should().BeOfType<InvalidOperationException>();
        roundtripResult.RetryAfter.Should().BeNull();
    }
#endif

    private const string TestMessage = "Dummy.";
    private static readonly TimeSpan TestRetryAfter = TimeSpan.FromHours(1);
}
