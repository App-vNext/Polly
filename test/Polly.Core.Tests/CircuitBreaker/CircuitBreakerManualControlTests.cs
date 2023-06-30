using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerManualControlTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task IsolateAsync_NotInitialized_Ok(bool closedAfter)
    {
        using var control = new CircuitBreakerManualControl();
        await control.IsolateAsync();
        if (closedAfter)
        {
            await control.CloseAsync();
        }

        var isolated = false;

        control.Initialize(
            c =>
            {
                c.IsSynchronous.Should().BeTrue();
                isolated = true;
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            () => { });

        isolated.Should().Be(!closedAfter);
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Ok()
    {
        using var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.CloseAsync(CancellationToken.None))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task Initialize_Twice_Ok()
    {
        int called = 0;
        var control = new CircuitBreakerManualControl();
        control.Initialize(_ => Task.CompletedTask, _ => Task.CompletedTask, () => { });
        control.Initialize(_ => { called++; return Task.CompletedTask; }, _ => { called++; return Task.CompletedTask; }, () => { called++; });

        await control.IsolateAsync();
        await control.CloseAsync();

        control.Dispose();

        called.Should().Be(3);
    }

    [Fact]
    public async Task Initialize_Ok()
    {
        var control = new CircuitBreakerManualControl();
        var isolateCalled = false;
        var resetCalled = false;
        var disposeCalled = false;

        control.Initialize(
            context =>
            {
                context.IsVoid.Should().BeTrue();
                context.IsSynchronous.Should().BeFalse();
                isolateCalled = true;
                return Task.CompletedTask;
            },
            context =>
            {
                context.IsVoid.Should().BeTrue();
                context.IsSynchronous.Should().BeFalse();
                resetCalled = true;
                return Task.CompletedTask;
            },
            () => disposeCalled = true);

        await control.IsolateAsync(CancellationToken.None);
        await control.CloseAsync(CancellationToken.None);

        control.Dispose();

        isolateCalled.Should().BeTrue();
        resetCalled.Should().BeTrue();
        disposeCalled.Should().BeTrue();
    }
}
