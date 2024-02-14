using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class IsolatedCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new IsolatedCircuitException("Dummy.").Message.Should().Be("Dummy.");
        new IsolatedCircuitException().Message.Should().Be("The circuit is manually held open and is not allowing calls.");
        new IsolatedCircuitException("Dummy.", new InvalidOperationException()).Message.Should().Be("Dummy.");
        new IsolatedCircuitException("Dummy.", new InvalidOperationException()).InnerException.Should().BeOfType<InvalidOperationException>();
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok() =>
        BinarySerializationUtil.SerializeAndDeserializeException(new IsolatedCircuitException("dummy")).Should().NotBeNull();
#endif
}
