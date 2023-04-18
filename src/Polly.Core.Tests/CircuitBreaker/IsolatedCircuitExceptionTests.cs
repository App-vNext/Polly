using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class IsolatedCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new IsolatedCircuitException("Dummy.").Message.Should().Be("Dummy.");
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok()
    {
        BinarySerializationUtil.SerializeAndDeserializeException(new IsolatedCircuitException("dummy")).Should().NotBeNull();
    }
#endif
}
