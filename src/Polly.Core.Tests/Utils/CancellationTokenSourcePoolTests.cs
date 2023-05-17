using System;
using System.Threading;
using Moq;
using Polly.Core.Tests.Helpers;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.Utils;

public class CancellationTokenSourcePoolTests
{
    public static IEnumerable<object[]> TimeProviders()
    {
        yield return new object[] { TimeProvider.System };
        yield return new object[] { new FakeTimeProvider() };
    }

    [Fact]
    public void ArgValidation_Ok()
    {
        var pool = CancellationTokenSourcePool.Create(TimeProvider.System);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Get(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Get(TimeSpan.FromMilliseconds(-2)));

        pool.Get(System.Threading.Timeout.InfiniteTimeSpan).Should().NotBeNull();
    }

    [MemberData(nameof(TimeProviders))]
    [Theory]
    public void RentReturn_Reusable_EnsureProperBehavior(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        pool.Return(cts);

        var cts2 = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);
#if NET6_0_OR_GREATER
        if (timeProvider == TimeProvider.System)
        {
            cts2.Should().BeSameAs(cts);
        }
        else
        {
            cts2.Should().NotBeSameAs(cts);
        }
#else
        cts2.Should().NotBeSameAs(cts);
#endif
    }

    [MemberData(nameof(TimeProviders))]
    [Theory]
    public void RentReturn_NotReusable_EnsureProperBehavior(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        cts.Cancel();
        pool.Return(cts);

        cts.Invoking(c => c.Token).Should().Throw<ObjectDisposedException>();

        var cts2 = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        cts2.Token.Should().NotBeNull();
    }

    [MemberData(nameof(TimeProviders))]
    [Theory]
    public async Task Rent_Cancellable_EnsureCancelled(object timeProvider)
    {
        if (timeProvider is Mock<TimeProvider> fakeTimeProvider)
        {
            fakeTimeProvider
                .Setup(v => v.CancelAfter(It.IsAny<CancellationTokenSource>(), TimeSpan.FromMilliseconds(1)))
                .Callback<CancellationTokenSource, TimeSpan>((source, _) => source.Cancel());
        }
        else
        {
            fakeTimeProvider = null!;
        }

        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(TimeSpan.FromMilliseconds(1));

        await Task.Delay(100);

        cts.IsCancellationRequested.Should().BeTrue();
        fakeTimeProvider?.VerifyAll();
    }

    [MemberData(nameof(TimeProviders))]
    [Theory]
    public async Task Rent_NotCancellable_EnsureNotCancelled(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);

        await Task.Delay(20);

        cts.IsCancellationRequested.Should().BeFalse();

        if (timeProvider is Mock<TimeProvider> fakeTimeProvider)
        {
            fakeTimeProvider
                .Verify(v => v.CancelAfter(It.IsAny<CancellationTokenSource>(), It.IsAny<TimeSpan>()), Times.Never());
        }
    }

    private static TimeProvider GetTimeProvider(object timeProvider) => timeProvider switch
    {
        Mock<TimeProvider> m => m.Object,
        _ => (TimeProvider)timeProvider
    };
}
