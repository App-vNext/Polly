using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BrokenCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new BrokenCircuitException().Message.Should().Be("The circuit is now open and is not allowing calls.");
        new BrokenCircuitException("Dummy.").Message.Should().Be("Dummy.");
        new BrokenCircuitException("Dummy.", new InvalidOperationException()).Message.Should().Be("Dummy.");
        new BrokenCircuitException("Dummy.", new InvalidOperationException()).InnerException.Should().BeOfType<InvalidOperationException>();
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok()
    {
        BinarySerializationUtil.SerializeAndDeserializeException(new BrokenCircuitException()).Should().NotBeNull();
    }
#endif
}
