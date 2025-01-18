using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class IsolatedCircuitExceptionTests
{
    [Fact]
    public void Ctor_Default_Ok()
    {
        var exception = new IsolatedCircuitException();
        exception.Message.ShouldBe("The circuit is manually held open and is not allowing calls.");
        exception.RetryAfter.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_Ok()
    {
        var exception = new IsolatedCircuitException(TestMessage);
        exception.Message.ShouldBe(TestMessage);
        exception.RetryAfter.ShouldBeNull();
    }

    [Fact]
    public void Ctor_Message_InnerException_Ok()
    {
        var exception = new IsolatedCircuitException(TestMessage, new InvalidOperationException());
        exception.Message.ShouldBe(TestMessage);
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.RetryAfter.ShouldBeNull();
    }

#if NETFRAMEWORK
    [Fact]
    public void BinarySerialization_Ok()
    {
        var exception = new IsolatedCircuitException(TestMessage, new InvalidOperationException());
        IsolatedCircuitException roundtripResult = BinarySerializationUtil.SerializeAndDeserializeException(exception);
        roundtripResult.ShouldNotBeNull();
        roundtripResult.Message.ShouldBe(TestMessage);
        roundtripResult.InnerException.ShouldBeOfType<InvalidOperationException>();
        roundtripResult.RetryAfter.ShouldBeNull();
    }
#endif

    private const string TestMessage = "Dummy.";
}
