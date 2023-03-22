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
}
