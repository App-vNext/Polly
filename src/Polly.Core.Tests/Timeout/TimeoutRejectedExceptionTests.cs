using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var delay = TimeSpan.FromSeconds(4);

        new TimeoutRejectedException().Message.Should().Be("Exception of type 'Polly.Timeout.TimeoutRejectedException' was thrown.");
        new TimeoutRejectedException("dummy").Message.Should().Be("dummy");
        new TimeoutRejectedException("dummy", new InvalidOperationException()).Message.Should().Be("dummy");
        new TimeoutRejectedException(delay).Timeout.Should().Be(delay);
        new TimeoutRejectedException("dummy", delay).Timeout.Should().Be(delay);
        new TimeoutRejectedException("dummy", delay, new InvalidOperationException()).Timeout.Should().Be(delay);
    }

#if !NETCOREAPP
    [Fact]
    public void BinaryDeserialization_Ok()
    {
        var timeout = TimeSpan.FromSeconds(4);

        var result = BinarySerializationUtil.SerializeAndDeserializeException(new TimeoutRejectedException(timeout));

        result.Timeout.Should().Be(timeout);
    }
#endif
}
