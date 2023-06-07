using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerManualControlTests
{
    [Fact]
    public void Ctor_Ok()
    {
        using var control = new CircuitBreakerManualControl();

        control.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task IsolateAsync_NotInitialized_Throws()
    {
        using var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.IsolateAsync(CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Throws()
    {
        using var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.CloseAsync(CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Twice_Throws()
    {
        using var control = new CircuitBreakerManualControl();
        control.Initialize(_ => Task.CompletedTask, _ => Task.CompletedTask, () => { });

        control
            .Invoking(c => c.Initialize(_ => Task.CompletedTask, _ => Task.CompletedTask, () => { }))
            .Should()
            .Throw<InvalidOperationException>();
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
