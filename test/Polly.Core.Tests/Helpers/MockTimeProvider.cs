using Moq;

namespace Polly.Core.Tests.Helpers;

internal class MockTimeProvider : Mock<TimeProvider>
{
    public MockTimeProvider()
        : base(MockBehavior.Strict)
    {
    }

    public MockTimeProvider SetupAnyDelay(CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(It.IsAny<TimeSpan>(), cancellationToken)).Returns(Task.CompletedTask);
        return this;
    }

    public MockTimeProvider SetupDelay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(delay, cancellationToken)).Returns(Task.CompletedTask);
        return this;
    }

    public MockTimeProvider SetupDelayCancelled(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(delay, cancellationToken)).ThrowsAsync(new OperationCanceledException());
        return this;
    }
}
