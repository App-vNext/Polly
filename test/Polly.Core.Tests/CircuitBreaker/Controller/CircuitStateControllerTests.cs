using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.CircuitBreaker;
using Polly.Telemetry;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class CircuitStateControllerTests
{
    private readonly FakeTimeProvider _timeProvider = new();

    private readonly CircuitBreakerStrategyOptions<int> _options = new();
    private readonly CircuitBehavior _circuitBehavior = Substitute.For<CircuitBehavior>();
    private readonly Action<TelemetryEventArguments<object, object>> _onTelemetry = _ => { };

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        using var controller = CreateController();

        controller.CircuitState.Should().Be(CircuitState.Closed);
        controller.LastException.Should().BeNull();
        controller.LastHandledOutcome.Should().BeNull();
    }

    [Fact]
    public async Task IsolateAsync_Ok()
    {
        // arrange
        bool called = false;
        _options.OnOpened = args =>
        {
            args.Arguments.BreakDuration.Should().Be(TimeSpan.MaxValue);
            args.Context.IsSynchronous.Should().BeFalse();
            args.Context.IsVoid.Should().BeFalse();
            args.Context.ResultType.Should().Be(typeof(int));
            args.Arguments.IsManual.Should().BeTrue();
            args.Outcome.IsVoidResult.Should().BeFalse();
            args.Outcome.Result.Should().Be(0);
            called = true;
            return default;
        };

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        using var controller = CreateController();
        var context = ResilienceContextPool.Shared.Get();

        // act
        await controller.IsolateCircuitAsync(context);

        // assert
        controller.CircuitState.Should().Be(CircuitState.Isolated);
        called.Should().BeTrue();

        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
        outcome.Value.Exception.Should().BeOfType<IsolatedCircuitException>();

        // now close it
        await controller.CloseCircuitAsync(ResilienceContextPool.Shared.Get());
        await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());

        _circuitBehavior.Received().OnCircuitClosed();
        context.ResilienceEvents.Should().Contain(new ResilienceEvent(ResilienceEventSeverity.Error, "OnCircuitOpened"));
    }

    [Fact]
    public async Task BreakAsync_Ok()
    {
        // arrange
        bool called = false;
        _options.OnClosed = args =>
        {
            args.Context.IsSynchronous.Should().BeFalse();
            args.Context.IsVoid.Should().BeFalse();
            args.Context.ResultType.Should().Be(typeof(int));
            args.Arguments.IsManual.Should().BeTrue();
            args.Outcome.IsVoidResult.Should().BeFalse();
            args.Outcome.Result.Should().Be(0);
            called = true;
            return default;
        };

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        using var controller = CreateController();
        await controller.IsolateCircuitAsync(ResilienceContextPool.Shared.Get());
        var context = ResilienceContextPool.Shared.Get();

        // act
        await controller.CloseCircuitAsync(context);

        // assert
        called.Should().BeTrue();

        await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
        _circuitBehavior.Received().OnCircuitClosed();
        context.ResilienceEvents.Should().Contain(new ResilienceEvent(ResilienceEventSeverity.Information, "OnCircuitClosed"));
    }

    [Fact]
    public async Task Disposed_EnsureThrows()
    {
        var controller = CreateController();
        controller.Dispose();

        Assert.Throws<ObjectDisposedException>(() => controller.CircuitState);
        Assert.Throws<ObjectDisposedException>(() => controller.LastException);
        Assert.Throws<ObjectDisposedException>(() => controller.LastHandledOutcome);

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.CloseCircuitAsync(ResilienceContextPool.Shared.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.IsolateCircuitAsync(ResilienceContextPool.Shared.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionSuccessAsync(Outcome.FromResult(10), ResilienceContextPool.Shared.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionFailureAsync(Outcome.FromResult(10), ResilienceContextPool.Shared.Get()));
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByValue()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromResult(99));
        var error = (BrokenCircuitException<int>)(await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get())).Value.Exception!;
        error.Should().BeOfType<BrokenCircuitException<int>>();
        error.Result.Should().Be(99);

        GetBlockedTill(controller).Should().Be(_timeProvider.GetUtcNow() + _options.BreakDuration);
    }

    [Fact]
    public async Task HalfOpen_EnsureBreakDuration()
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);
        GetBlockedTill(controller).Should().Be(_timeProvider.GetUtcNow() + _options.BreakDuration);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HalfOpen_EnsureCorrectStateTransitionAfterExecution(bool success)
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);

        if (success)
        {
            await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get());
            controller.CircuitState.Should().Be(CircuitState.Closed);

            _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
            _circuitBehavior.Received().OnCircuitClosed();
        }
        else
        {
            await controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get());
            controller.CircuitState.Should().Be(CircuitState.Open);

            _circuitBehavior.DidNotReceiveWithAnyArgs().OnActionSuccess(default);
            _circuitBehavior.Received().OnActionFailure(CircuitState.HalfOpen, out _);
        }
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByException()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromException<int>(new InvalidOperationException()));
        var error = (BrokenCircuitException)(await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get())).Value.Exception!;
        error.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task OnActionFailure_EnsureLock()
    {
        // arrange
        using var executing = new ManualResetEvent(false);
        using var verified = new ManualResetEvent(false);

        AdvanceTime(_options.BreakDuration);
        bool shouldBreak = false;
        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Closed, out shouldBreak)).Do((_) =>
        {
            executing.Set();
            verified.WaitOne();
        });

        using var controller = CreateController();

        // act
        var executeAction = Task.Run(() => controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get()));
        executing.WaitOne();
        var executeAction2 = Task.Run(() => controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get()));

        // assert
        executeAction.Wait(50).Should().BeFalse();
        verified.Set();
        await executeAction;
        await executeAction2;
    }

    [Fact]
    public async Task OnActionPreExecute_HalfOpen()
    {
        // arrange
        var called = false;
        _options.OnHalfOpened = _ =>
        {
            called = true;
            return default;
        };

        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromResult(10));
        AdvanceTime(_options.BreakDuration);

        // act
        await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
        var error = (await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get())).Value.Exception;
        error.Should().BeOfType<BrokenCircuitException<int>>();

        // assert
        controller.CircuitState.Should().Be(CircuitState.HalfOpen);
        called.Should().BeTrue();
    }

    [InlineData(CircuitState.HalfOpen, CircuitState.Closed)]
    [InlineData(CircuitState.Isolated, CircuitState.Isolated)]
    [InlineData(CircuitState.Closed, CircuitState.Closed)]
    [Theory]
    public async Task OnActionSuccess_EnsureCorrectBehavior(CircuitState state, CircuitState expectedState)
    {
        // arrange
        var called = false;
        _options.OnClosed = args =>
        {
            args.Arguments.IsManual.Should().BeFalse();
            called = true;
            return default;
        };

        using var controller = CreateController();

        await TransitionToState(controller, state);

        // act
        await controller.OnActionSuccessAsync(Outcome.FromResult(10), ResilienceContextPool.Shared.Get());

        // assert
        controller.CircuitState.Should().Be(expectedState);

        _circuitBehavior.Received().OnActionSuccess(state);
        if (expectedState == CircuitState.Closed && state != CircuitState.Closed)
        {
            _circuitBehavior.Received().OnCircuitClosed();
            called.Should().BeTrue();
        }
    }

    [InlineData(CircuitState.HalfOpen, CircuitState.Open, true)]
    [InlineData(CircuitState.Closed, CircuitState.Open, true)]
    [InlineData(CircuitState.Closed, CircuitState.Closed, false)]
    [InlineData(CircuitState.Open, CircuitState.Open, false)]
    [InlineData(CircuitState.Isolated, CircuitState.Isolated, false)]
    [Theory]
    public async Task OnActionFailureAsync_EnsureCorrectBehavior(CircuitState state, CircuitState expectedState, bool shouldBreak)
    {
        // arrange
        var called = false;
        _options.OnOpened = args =>
        {
            if (state == CircuitState.Isolated)
            {
                args.Arguments.IsManual.Should().BeTrue();
            }
            else
            {
                args.Arguments.IsManual.Should().BeFalse();
            }

            called = true;
            return default;
        };
        using var controller = CreateController();

        await TransitionToState(controller, state);

        _circuitBehavior.When(x => x.OnActionFailure(state, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContextPool.Shared.Get());

        // assert
        controller.LastHandledOutcome!.Value.Result.Should().Be(99);
        controller.CircuitState.Should().Be(expectedState);
        _circuitBehavior.Received().OnActionFailure(state, out Arg.Any<bool>());

        if (expectedState == CircuitState.Open && state != CircuitState.Open)
        {
            called.Should().BeTrue();
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task OnActionFailureAsync_EnsureBreakDurationNotOverflow(bool overflow)
    {
        // arrange
        using var controller = CreateController();
        var shouldBreak = true;
        await TransitionToState(controller, CircuitState.HalfOpen);
        var utcNow = DateTime.MaxValue - _options.BreakDuration;
        if (overflow)
        {
            utcNow += TimeSpan.FromMilliseconds(10);
        }

        _timeProvider.SetUtcNow(utcNow);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.HalfOpen, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContextPool.Shared.Get());

        // assert
        var blockedTill = GetBlockedTill(controller);

        if (overflow)
        {
            blockedTill.Should().Be(DateTimeOffset.MaxValue);
        }
        else
        {
            blockedTill.Should().Be(utcNow + _options.BreakDuration);
        }
    }

    [Fact]
    public async Task OnActionFailureAsync_VoidResult_EnsureBreakingExceptionNotSet()
    {
        // arrange
        using var controller = CreateController();
        bool shouldBreak = true;
        await TransitionToState(controller, CircuitState.Open);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Open, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContextPool.Shared.Get());

        // assert
        controller.LastException.Should().BeNull();
        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException<int>>();
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Closed()
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);

        await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get());
        controller.CircuitState.Should().Be(CircuitState.Closed);

        _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
        _circuitBehavior.Received().OnCircuitClosed();
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Open_HalfOpen_Closed()
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();
        bool shouldBreak = true;

        await TransitionToState(controller, CircuitState.HalfOpen);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.HalfOpen, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        await controller.OnActionFailureAsync(Outcome.FromResult(0), context);
        controller.CircuitState.Should().Be(CircuitState.Open);

        // execution rejected
        AdvanceTime(TimeSpan.FromMilliseconds(1));
        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException<int>>();

        // wait and try, transition to half open
        AdvanceTime(_options.BreakDuration + _options.BreakDuration);
        await controller.OnActionPreExecuteAsync(context);
        controller.CircuitState.Should().Be(CircuitState.HalfOpen);

        // close circuit
        await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContextPool.Shared.Get());
        controller.CircuitState.Should().Be(CircuitState.Closed);

        _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
        _circuitBehavior.Received().OnCircuitClosed();
    }

    [Fact]
    public async Task ExecuteScheduledTask_Async_Ok()
    {
        var source = new TaskCompletionSource<string>();
        var task = CircuitStateController<string>.ExecuteScheduledTaskAsync(source.Task, ResilienceContextPool.Shared.Get().Initialize<string>(isSynchronous: false)).AsTask();

        task.Wait(3).Should().BeFalse();
        task.IsCompleted.Should().BeFalse();

        source.SetResult("ok");

        await task;
        task.IsCompleted.Should().BeTrue();
    }

    private static DateTimeOffset? GetBlockedTill(CircuitStateController<int> controller) =>
        (DateTimeOffset?)controller.GetType().GetField("_blockedUntil", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)!;

    private async Task TransitionToState(CircuitStateController<int> controller, CircuitState state)
    {
        switch (state)
        {
            case CircuitState.Closed:
                break;
            case CircuitState.Open:
                await OpenCircuit(controller);
                break;
            case CircuitState.HalfOpen:
                await OpenCircuit(controller);
                AdvanceTime(_options.BreakDuration);
                await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get());
                break;
            case CircuitState.Isolated:
                await controller.IsolateCircuitAsync(ResilienceContextPool.Shared.Get());
                break;
        }

        controller.CircuitState.Should().Be(state);
    }

    private async Task OpenCircuit(CircuitStateController<int> controller, Outcome<int>? outcome = null)
    {
        bool breakCircuit = true;

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
                        .Do(x => x[1] = breakCircuit);

        await controller.OnActionFailureAsync(outcome ?? Outcome.FromResult(10), ResilienceContextPool.Shared.Get().Initialize<int>(true));
    }

    private void AdvanceTime(TimeSpan timespan) => _timeProvider.Advance(timespan);

    private CircuitStateController<int> CreateController() => new(
        _options.BreakDuration,
        _options.OnOpened,
        _options.OnClosed,
        _options.OnHalfOpened,
        _circuitBehavior,
        _timeProvider,
        TestUtilities.CreateResilienceTelemetry(_onTelemetry.Invoke));
}
