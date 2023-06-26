using Moq;

namespace Polly.Core.Tests.Helpers;

internal class MockTimeProvider : Mock<TimeProvider>
{
    public MockTimeProvider()
        : base(MockBehavior.Strict)
    {
    }

    public MockTimeProvider SetupAnyCreateTimer()
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

    public MockTimeProvider VerifyCreateTimer(Times times)
    {
        Verify(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), times);
        return this;
    }

    public MockTimeProvider VerifyCreateTimer(TimeSpan delay, Times times)
    {
        Verify(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), delay, It.IsAny<TimeSpan>()), times);
        return this;
    }

    public MockTimeProvider SetupCreateTimerException(TimeSpan delay, Exception exception)
    {
        Setup(t => t.CreateTimer(It.IsAny<TimerCallback>(), It.IsAny<object?>(), delay, It.IsAny<TimeSpan>()))
        .Callback((TimerCallback _, object? _, TimeSpan _, TimeSpan _) => throw exception)
        .Returns(Of<ITimer>());

        return this;
    }
}
