using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BrokenCircuitExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new BrokenCircuitException("Dummy.").Message.Should().Be("Dummy.");
        new BrokenCircuitException("Dummy.", new InvalidOperationException()).Message.Should().Be("Dummy.");
        new BrokenCircuitException("Dummy.", new InvalidOperationException()).InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void Ctor_Generic_Ok()
    {
        var exception = new BrokenCircuitException<int>(10);
        exception.Result.Should().Be(10);

        exception = new BrokenCircuitException<int>("Dummy.", 10);
        exception.Message.Should().Be("Dummy.");
        exception.Result.Should().Be(10);

        exception = new BrokenCircuitException<int>("Dummy.", new InvalidOperationException(), 10);
        exception.Message.Should().Be("Dummy.");
        exception.Result.Should().Be(10);
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

#if !NETCOREAPP
    [Fact]
    public void BinarySerialization_Ok()
    {
        BinarySerializationUtil.SerializeAndDeserializeException(new BrokenCircuitException()).Should().NotBeNull();
    }

    [Fact]
    public void BinarySerialization_Generic_Ok()
    {
        var result = BinarySerializationUtil
            .SerializeAndDeserializeException(new BrokenCircuitException<int>(123));

        result.Should().NotBeNull();

        // default
        result.Result.Should().Be(0);
    }
#endif
}
