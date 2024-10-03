using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BrokenCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var retryAfter = new TimeSpan(1, 0, 0);
        var defaultException = new BrokenCircuitException();
        defaultException.Message.Should().Be("The circuit is now open and is not allowing calls.");
        defaultException.RetryAfter.Should().BeNull();
        var retryAfterException = new BrokenCircuitException(retryAfter);
        retryAfterException.Message.Should().Be($"The circuit is now open and is not allowing calls. It can be retried after '{retryAfter}'.");
        retryAfterException.RetryAfter.Should().Be(retryAfter);
        var dummyMessageException = new BrokenCircuitException("Dummy.");
        dummyMessageException.Message.Should().Be("Dummy.");
        dummyMessageException.RetryAfter.Should().BeNull();
        var dummyMessageWithRetryAfterException = new BrokenCircuitException("Dummy.", retryAfter);
        dummyMessageWithRetryAfterException.Message.Should().Be("Dummy.");
        dummyMessageWithRetryAfterException.RetryAfter.Should().Be(retryAfter);
        var dummyMessageExceptionWithInnerException = new BrokenCircuitException("Dummy.", new InvalidOperationException());
        dummyMessageExceptionWithInnerException.Message.Should().Be("Dummy.");
        dummyMessageExceptionWithInnerException.InnerException.Should().BeOfType<InvalidOperationException>();
        dummyMessageExceptionWithInnerException.RetryAfter.Should().BeNull();
        var dummyMessageExceptionWithInnerExceptionAndRetryAfter = new BrokenCircuitException("Dummy.", retryAfter, new InvalidOperationException());
        dummyMessageExceptionWithInnerExceptionAndRetryAfter.Message.Should().Be("Dummy.");
        dummyMessageExceptionWithInnerExceptionAndRetryAfter.InnerException.Should().BeOfType<InvalidOperationException>();
        dummyMessageExceptionWithInnerExceptionAndRetryAfter.RetryAfter.Should().Be(retryAfter);
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok() =>
        BinarySerializationUtil.SerializeAndDeserializeException(new BrokenCircuitException()).Should().NotBeNull();
#endif
}
