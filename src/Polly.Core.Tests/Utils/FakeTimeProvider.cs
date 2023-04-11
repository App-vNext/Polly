using Moq;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

internal class FakeTimeProvider : Mock<TimeProvider>
{
    public FakeTimeProvider(long frequency)
    : base(MockBehavior.Strict, frequency)
    {
    }

    public FakeTimeProvider()
        : this(Stopwatch.Frequency)
    {
    }

    public FakeTimeProvider SetupAnyDelay(CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(It.IsAny<TimeSpan>(), cancellationToken)).Returns(Task.CompletedTask);
        return this;
    }

    public FakeTimeProvider SetupDelay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(delay, cancellationToken)).Returns(Task.CompletedTask);
        return this;
    }

    public FakeTimeProvider SetupDelayCancelled(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(delay, cancellationToken)).ThrowsAsync(new OperationCanceledException());
        return this;
    }

    public FakeTimeProvider SetupCancelAfterNow(TimeSpan delay)
    {
        Setup(v => v.CancelAfter(It.IsAny<CancellationTokenSource>(), delay)).Callback<CancellationTokenSource, TimeSpan>((cts, _) => cts.Cancel());
        return this;
    }
}
