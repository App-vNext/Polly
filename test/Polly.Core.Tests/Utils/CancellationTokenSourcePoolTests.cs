using Microsoft.Extensions.Time.Testing;
using Polly.Utils;

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
        var e = Assert.Throws<ArgumentOutOfRangeException>(() => pool.Get(TimeSpan.FromMilliseconds(-2)));
        e.Message.ShouldStartWith("Invalid delay specified.");
        e.ActualValue.ShouldBe(TimeSpan.FromMilliseconds(-2));

        pool.Get(System.Threading.Timeout.InfiniteTimeSpan).ShouldNotBeNull();
    }

#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(TimeProviders))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
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
            cts2.ShouldBeSameAs(cts);
        }
        else
        {
            cts2.ShouldNotBeSameAs(cts);
        }
#else
        cts2.ShouldNotBeSameAs(cts);
#endif
    }

#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(TimeProviders))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [Theory]
    public void RentReturn_NotReusable_EnsureProperBehavior(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        cts.Cancel();
        pool.Return(cts);

        Should.Throw<ObjectDisposedException>(() => cts.Token);

        var cts2 = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);

        Should.NotThrow(() => cts2.Token);
    }

#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(TimeProviders))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [Theory]
    public async Task Rent_Cancellable_EnsureCancelled(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(TimeSpan.FromMilliseconds(1));

        if (timeProvider is FakeTimeProvider fakeTimeProvider)
        {
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        await Task.Delay(100, TestCancellation.Token);

        await TestUtilities.AssertWithTimeoutAsync(() => cts.IsCancellationRequested.ShouldBeTrue());
    }

#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(TimeProviders))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [Theory]
    public async Task Rent_NotCancellable_EnsureNotCancelled(object timeProvider)
    {
        var pool = CancellationTokenSourcePool.Create(GetTimeProvider(timeProvider));
        var cts = pool.Get(System.Threading.Timeout.InfiniteTimeSpan);

        await Task.Delay(20, TestCancellation.Token);

        cts.IsCancellationRequested.ShouldBeFalse();
    }

    private static TimeProvider GetTimeProvider(object timeProvider) => (TimeProvider)timeProvider;
}
