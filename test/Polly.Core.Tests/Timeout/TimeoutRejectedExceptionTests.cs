using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var delay = TimeSpan.FromSeconds(4);

        new TimeoutRejectedException().Message.ShouldBe("The operation didn't complete within the allowed timeout.");
        new TimeoutRejectedException("dummy").Message.ShouldBe("dummy");
        new TimeoutRejectedException("dummy", new InvalidOperationException()).Message.ShouldBe("dummy");
        new TimeoutRejectedException(delay).Timeout.ShouldBe(delay);
        new TimeoutRejectedException(delay).Message.ShouldBe("The operation didn't complete within the allowed timeout.");
        new TimeoutRejectedException("dummy", delay).Timeout.ShouldBe(delay);
        new TimeoutRejectedException("dummy", delay, new InvalidOperationException()).Timeout.ShouldBe(delay);
    }

#if NETFRAMEWORK
    [Fact]
    public void BinaryDeserialization_Ok()
    {
        var timeout = TimeSpan.FromSeconds(4);

        var result = BinarySerializationUtil.SerializeAndDeserializeException(new TimeoutRejectedException(timeout));

        result.Timeout.ShouldBe(timeout);
    }
#endif
}
