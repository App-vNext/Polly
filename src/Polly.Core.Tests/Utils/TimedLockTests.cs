using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TimedLockTests
{
    [Fact]
    public void Lock_Ok()
    {
        var syncRoot = new object();

        using var inLock = new ManualResetEvent(false);
        using var inLock2 = new ManualResetEvent(false);
        using var verify = new ManualResetEvent(false);

        Task.Run(() =>
        {
            using var l = TimedLock.Lock(syncRoot);
            inLock.Set();
            verify.WaitOne();
        });

        inLock.WaitOne();

        Task.Run(() =>
        {
            using var l = TimedLock.Lock(syncRoot);
            inLock2.Set();
        });

        inLock2.WaitOne(100).Should().BeFalse();
        verify.Set();
        inLock2.WaitOne().Should().BeTrue();
    }
}
