using Moq;

namespace Polly.Core.Tests.Helpers;

internal class FakeTimeProvider : Mock<TimeProvider>
{
    private DateTimeOffset? _time;

    public FakeTimeProvider SetupTimestampFrequency(long? frequency = null)
    {
        Setup(v => v.TimestampFrequency).Returns(frequency ?? Stopwatch.Frequency);
        return this;
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

    public FakeTimeProvider SetupAnyCreateTimer()
    {
        Setup(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
        .Callback((TimerCallback callback, object? state, TimeSpan _, TimeSpan _) => callback(state))
        .Returns(Of<ITimer>());

        return this;
    }

    public Mock<ITimer> SetupCreateTimer(TimeSpan delay)
    {
        var timer = new Mock<ITimer>(MockBehavior.Loose);

        Setup(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), delay, It.IsAny<TimeSpan>()))
        .Callback((TimerCallback callback, object? state, TimeSpan _, TimeSpan _) => callback(state))
        .Returns(timer.Object);

        return timer;
    }

    public FakeTimeProvider VerifyCreateTimer(Times times)
    {
        Verify(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), times);
        return this;
    }

    public FakeTimeProvider VerifyCreateTimer(TimeSpan delay, Times times)
    {
        Verify(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), delay, It.IsAny<TimeSpan>()), times);
        return this;
    }

    public FakeTimeProvider SetupCreateTimerException(TimeSpan delay, Exception exception)
    {
        Setup(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), delay, It.IsAny<TimeSpan>()))
        .Callback((TimerCallback _, object? _, TimeSpan _, TimeSpan _) => throw exception)
        .Returns(Of<ITimer>());

        return this;
    }

}
