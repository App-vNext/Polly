using Moq;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

internal class FakeTimeProvider : Mock<TimeProvider>
{
    public FakeTimeProvider()
        : base(MockBehavior.Strict, Stopwatch.Frequency)
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
