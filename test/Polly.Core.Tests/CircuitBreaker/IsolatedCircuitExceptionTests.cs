using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class IsolatedCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var dummyMessageException = new IsolatedCircuitException("Dummy.");
        dummyMessageException.Message.Should().Be("Dummy.");
        dummyMessageException.RetryAfter.Should().BeNull();
        var defaultException = new IsolatedCircuitException();
        defaultException.Message.Should().Be("The circuit is manually held open and is not allowing calls.");
        defaultException.RetryAfter.Should().BeNull();
        var dummyMessageExceptionWithInnerException = new IsolatedCircuitException("Dummy.", new InvalidOperationException());
        dummyMessageExceptionWithInnerException.Message.Should().Be("Dummy.");
        dummyMessageExceptionWithInnerException.InnerException.Should().BeOfType<InvalidOperationException>();
        dummyMessageExceptionWithInnerException.RetryAfter.Should().BeNull();
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok() =>
        BinarySerializationUtil.SerializeAndDeserializeException(new IsolatedCircuitException("dummy")).Should().NotBeNull();
#endif
}
