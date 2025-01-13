using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerManualControlTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Ctor_Isolated(bool isolated)
    {
        var control = new CircuitBreakerManualControl(isolated);
        var isolateCalled = false;

        using var reg = control.Initialize(
            c =>
            {
                c.IsSynchronous.Should().BeTrue();
                isolateCalled = true;
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask);

        isolateCalled.Should().Be(isolated);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task IsolateAsync_NotInitialized_Ok(bool closedAfter)
    {
        var cancellationToken = CancellationToken.None;
        var control = new CircuitBreakerManualControl();
        await control.IsolateAsync(cancellationToken);
        if (closedAfter)
        {
            await control.CloseAsync(cancellationToken);
        }

        var isolated = false;

        using var reg = control.Initialize(
            c =>
            {
                c.IsSynchronous.Should().BeTrue();
                isolated = true;
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask);

        isolated.Should().Be(!closedAfter);
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Ok()
    {
        var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.CloseAsync())
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task Initialize_Twice_Ok()
    {
        int called = 0;
        var cancellationToken = CancellationToken.None;
        var control = new CircuitBreakerManualControl();
        control.Initialize(_ => Task.CompletedTask, _ => Task.CompletedTask);
        control.Initialize(_ => { called++; return Task.CompletedTask; }, _ => { called++; return Task.CompletedTask; });

        await control.IsolateAsync(cancellationToken);
        await control.CloseAsync(cancellationToken);

        called.Should().Be(2);
    }

    [Fact]
    public async Task Initialize_DisposeRegistration_ShuldBeCancelled()
    {
        int called = 0;
        var cancellationToken = CancellationToken.None;
        var control = new CircuitBreakerManualControl();
        var reg = control.Initialize(_ => { called++; return Task.CompletedTask; }, _ => { called++; return Task.CompletedTask; });

        await control.IsolateAsync(cancellationToken);
        await control.CloseAsync(cancellationToken);

        reg.Dispose();

        await control.IsolateAsync(cancellationToken);
        await control.CloseAsync(cancellationToken);

        called.Should().Be(2);
    }

    [Fact]
    public async Task Initialize_Ok()
    {
        var cancellationToken = CancellationToken.None;
        var control = new CircuitBreakerManualControl();
        var isolateCalled = false;
        var resetCalled = false;

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
            });

        await control.IsolateAsync(cancellationToken);
        await control.CloseAsync(cancellationToken);

        isolateCalled.Should().BeTrue();
        resetCalled.Should().BeTrue();
    }
}
