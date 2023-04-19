using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TimedLockTests
{
    [Fact]
    public async Task Lock_Ok()
    {
        var syncRoot = new object();

        using var inLock = new ManualResetEvent(false);
        using var inLock2 = new ManualResetEvent(false);
        using var verify = new ManualResetEvent(false);

        var t1 = Task.Run(() =>
        {
            using var l = TimedLock.Lock(syncRoot);
            inLock.Set();
            verify.WaitOne();
        });

        inLock.WaitOne();

        var t2 = Task.Run(() =>
        {
            using var l = TimedLock.Lock(syncRoot);
            inLock2.Set();
        });

        inLock2.WaitOne(100).Should().BeFalse();
        verify.Set();
        inLock2.WaitOne().Should().BeTrue();

        await t1;
        await t2;
    }
}
