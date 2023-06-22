using Moq;

namespace Polly.Core.Tests.Helpers;

internal class FakeTimeProvider : Mock<TimeProvider>
{
    private DateTimeOffset? _time;

    public FakeTimeProvider(long frequency)
        : base(MockBehavior.Strict, frequency)
    {
    }

    public FakeTimeProvider()
        : this(Stopwatch.Frequency)
    {
    }

    public FakeTimeProvider SetupUtcNow(DateTimeOffset? time = null)
    {
        _time = time ?? DateTimeOffset.UtcNow;
        Setup(x => x.GetUtcNow()).Returns(() => _time.Value);
        return this;
    }

    public FakeTimeProvider AdvanceTime(TimeSpan time)
    {
        if (_time == null)
        {
            SetupUtcNow(DateTimeOffset.UtcNow);
        }

        _time = _time!.Value.Add(time);
        return this;
    }

    public FakeTimeProvider SetupAnyDelay(CancellationToken cancellationToken = default)
    {
        Setup(x => x.Delay(It.IsAny<TimeSpan>(), cancellationToken)).Returns(Task.CompletedTask);
        return this;
    }

    public FakeTimeProvider SetupGetTimestamp()
    {
        Setup(x => x.GetTimestamp()).Returns(0);
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
