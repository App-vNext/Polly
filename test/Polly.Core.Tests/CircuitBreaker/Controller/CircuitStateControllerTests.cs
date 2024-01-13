using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class CircuitStateControllerTests
{
    private readonly FakeTimeProvider _timeProvider = new();

    private readonly CircuitBreakerStrategyOptions<int> _options = new();
    private readonly CircuitBehavior _circuitBehavior = Substitute.For<CircuitBehavior>();
    private readonly FakeTelemetryListener _telemetryListener = new();

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
            args.BreakDuration.Should().Be(TimeSpan.MaxValue);
            args.Context.IsSynchronous.Should().BeFalse();
            args.Context.IsVoid.Should().BeFalse();
            args.Context.ResultType.Should().Be(typeof(int));
            args.IsManual.Should().BeTrue();
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
        _telemetryListener.GetArgs<OnCircuitOpenedArguments<int>>().Should().NotBeEmpty();
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
            args.IsManual.Should().BeTrue();
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
        _telemetryListener.GetArgs<OnCircuitClosedArguments<int>>().Should().NotBeEmpty();
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
        var error = (BrokenCircuitException)(await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get())).Value.Exception!;
        error.Should().BeOfType<BrokenCircuitException>();

        GetBlockedTill(controller).Should().Be(_timeProvider.GetUtcNow() + _options.BreakDuration);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task OnActionPreExecute_CircuitOpened_EnsureExceptionStackTraceDoesNotGrow(bool innerException)
    {
        var stacks = new List<string>();
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await OpenCircuit(
            controller,
            innerException ? Outcome.FromException<int>(new InvalidOperationException()) : Outcome.FromResult(99));

        for (int i = 0; i < 100; i++)
        {
            try
            {
                (await controller.OnActionPreExecuteAsync(context)).Value.ThrowIfException();
            }
            catch (BrokenCircuitException e)
            {
                stacks.Add(e.StackTrace!);
                e.Message.Should().Be("The circuit is now open and is not allowing calls.");

                if (innerException)
                {
                    e.InnerException.Should().BeOfType<InvalidOperationException>();
                }
                else
                {
                    e.InnerException.Should().BeNull();
                }
            }
        }

        stacks.Distinct().Should().HaveCount(1);
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
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        executeAction.Wait(50).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
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
        error.Should().BeOfType<BrokenCircuitException>();

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
            args.IsManual.Should().BeFalse();
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
                args.IsManual.Should().BeTrue();
            }
            else
            {
                args.IsManual.Should().BeFalse();
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

    [Fact]
    public async Task OnActionFailureAsync_EnsureBreakDurationGeneration()
    {
        // arrange
        _options.BreakDurationGenerator = static args =>
        {
            args.FailureCount.Should().Be(1);
            args.FailureRate.Should().Be(0.5);
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(42));
        };

        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.Closed);

        var utcNow = new DateTimeOffset(2023, 12, 12, 12, 34, 56, TimeSpan.Zero);
        _timeProvider.SetUtcNow(utcNow);

        _circuitBehavior.FailureCount.Returns(1);
        _circuitBehavior.FailureRate.Returns(0.5);
        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
            .Do(x => x[1] = true);

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContextPool.Shared.Get());

        // assert
        var blockedTill = GetBlockedTill(controller);
        blockedTill.Should().Be(utcNow + TimeSpan.FromMinutes(42));
    }

    [Fact]
    public async Task BreakDurationGenerator_EnsureHalfOpenAttempts()
    {
        // arrange
        var halfOpenAttempts = new List<int>();

        _options.BreakDurationGenerator = args =>
        {
            halfOpenAttempts.Add(args.HalfOpenAttempts);
            return new ValueTask<TimeSpan>(TimeSpan.Zero);
        };

        using var controller = CreateController();

        // act
        await TransitionToState(controller, CircuitState.Closed);

        for (int i = 0; i < 5; i++)
        {
            await TransitionToState(controller, CircuitState.Open);
            await TransitionToState(controller, CircuitState.HalfOpen);
        }

        await TransitionToState(controller, CircuitState.Closed);

        for (int i = 0; i < 3; i++)
        {
            await TransitionToState(controller, CircuitState.Open);
            await TransitionToState(controller, CircuitState.HalfOpen);
        }

        // assert
        halfOpenAttempts.Should().BeEquivalentTo([0, 1, 2, 3, 4, 0, 1, 2]);
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
        var utcNow = DateTimeOffset.MaxValue - _options.BreakDuration;
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
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException>();
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
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException>();

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

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        task.Wait(3).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
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
                await controller.CloseCircuitAsync(ResilienceContextPool.Shared.Get());
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
        TestUtilities.CreateResilienceTelemetry(_telemetryListener),
        _options.BreakDurationGenerator);
}
