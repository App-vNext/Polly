using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class CancellationTokenSourcePoolTests
{
    [Fact]
    public void RentReturn_Reusable_EnsureProperBehavior()
    {
        var cts = CancellationTokenSourcePool.Get();
        CancellationTokenSourcePool.Return(cts);

        var cts2 = CancellationTokenSourcePool.Get();
#if NET6_0_OR_GREATER
        cts2.Should().BeSameAs(cts);
#else
        cts2.Should().NotBeSameAs(cts);
#endif
    }

    [Fact]
    public void RentReturn_NotReusable_EnsureProperBehavior()
    {
        var cts = CancellationTokenSourcePool.Get();
        cts.Cancel();
        CancellationTokenSourcePool.Return(cts);

        cts.Invoking(c => c.Token).Should().Throw<ObjectDisposedException>();

        var cts2 = CancellationTokenSourcePool.Get();
        cts2.Token.Should().NotBeNull();
    }
}
