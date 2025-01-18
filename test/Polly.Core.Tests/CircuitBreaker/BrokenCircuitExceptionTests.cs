using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BrokenCircuitExceptionTests
{
    [Fact]
    public void Ctor_Default_Ok()
    {
        var exception = new BrokenCircuitException();
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.RetryAfter.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage);
        exception.Message.ShouldBe(TestMessage);
        exception.RetryAfter.ShouldBeNull();
    }

    [Fact]
    public void Ctor_RetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestRetryAfter);
        exception.Message.ShouldBe($"The circuit is now open and is not allowing calls. It can be retried after '{TestRetryAfter}'.");
        exception.RetryAfter.ShouldBe(TestRetryAfter);
    }

    [Fact]
    public void Ctor_Message_RetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter);
        exception.Message.ShouldBe(TestMessage);
        exception.RetryAfter.ShouldBe(TestRetryAfter);
    }

    [Fact]
    public void Ctor_Message_InnerException_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, new InvalidOperationException());
        exception.Message.ShouldBe(TestMessage);
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.RetryAfter.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_RetryAfter_InnerException_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter, new InvalidOperationException());
        exception.Message.ShouldBe(TestMessage);
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.RetryAfter.ShouldBe(TestRetryAfter);
    }

#if NETFRAMEWORK
    [Fact]
    public void BinarySerialization_NonNullRetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, TestRetryAfter, new InvalidOperationException());
        BrokenCircuitException roundtripResult = BinarySerializationUtil.SerializeAndDeserializeException(exception);
        roundtripResult.ShouldNotBeNull();
        roundtripResult.Message.ShouldBe(TestMessage);
        roundtripResult.InnerException.ShouldBeOfType<InvalidOperationException>();
        roundtripResult.RetryAfter.ShouldBe(TestRetryAfter);
    }

    [Fact]
    public void BinarySerialization_NullRetryAfter_Ok()
    {
        var exception = new BrokenCircuitException(TestMessage, new InvalidOperationException());
        BrokenCircuitException roundtripResult = BinarySerializationUtil.SerializeAndDeserializeException(exception);
        roundtripResult.ShouldNotBeNull();
        roundtripResult.Message.ShouldBe(TestMessage);
        roundtripResult.InnerException.ShouldBeOfType<InvalidOperationException>();
        roundtripResult.RetryAfter.ShouldBeNull();
    }
#endif

    private const string TestMessage = "Dummy.";
    private static readonly TimeSpan TestRetryAfter = TimeSpan.FromHours(1);
}
