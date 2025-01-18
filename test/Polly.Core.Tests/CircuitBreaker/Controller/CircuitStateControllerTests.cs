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

        controller.CircuitState.ShouldBe(CircuitState.Closed);
        controller.LastException.ShouldBeNull();
        controller.LastHandledOutcome.ShouldBeNull();
    }

    [Fact]
    public async Task IsolateAsync_Ok()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        bool called = false;
        _options.OnOpened = args =>
        {
            args.BreakDuration.ShouldBe(TimeSpan.MaxValue);
            args.Context.IsSynchronous.ShouldBeFalse();
            args.Context.IsVoid.ShouldBeFalse();
            args.Context.ResultType.ShouldBe(typeof(int));
            args.IsManual.ShouldBeTrue();
            args.Outcome.IsVoidResult.ShouldBeFalse();
            args.Outcome.Result.ShouldBe(0);
            called = true;
            return default;
        };

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        using var controller = CreateController();
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        // act
        await controller.IsolateCircuitAsync(context);

        // assert
        controller.CircuitState.ShouldBe(CircuitState.Isolated);
        called.ShouldBeTrue();

        var outcome = await controller.OnActionPreExecuteAsync(context);
        var exception = outcome.Value.Exception.ShouldBeOfType<IsolatedCircuitException>();
        exception.RetryAfter.ShouldBeNull();
        exception.TelemetrySource.ShouldNotBeNull();

        // now close it
        await controller.CloseCircuitAsync(context);
        await controller.OnActionPreExecuteAsync(context);

        _circuitBehavior.Received().OnCircuitClosed();
        _telemetryListener.GetArgs<OnCircuitOpenedArguments<int>>().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task BreakAsync_Ok()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        bool called = false;
        _options.OnClosed = args =>
        {
            args.Context.IsSynchronous.ShouldBeFalse();
            args.Context.IsVoid.ShouldBeFalse();
            args.Context.ResultType.ShouldBe(typeof(int));
            args.IsManual.ShouldBeTrue();
            args.Outcome.IsVoidResult.ShouldBeFalse();
            args.Outcome.Result.ShouldBe(0);
            called = true;
            return default;
        };

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        using var controller = CreateController();
        await controller.IsolateCircuitAsync(ResilienceContextPool.Shared.Get(cancellationToken));
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        // act
        await controller.CloseCircuitAsync(context);

        // assert
        called.ShouldBeTrue();

        await controller.OnActionPreExecuteAsync(ResilienceContextPool.Shared.Get(cancellationToken));
        _circuitBehavior.Received().OnCircuitClosed();
        _telemetryListener.GetArgs<OnCircuitClosedArguments<int>>().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Disposed_EnsureThrows()
    {
        var context = ResilienceContextPool.Shared.Get();

        var controller = CreateController();
        controller.Dispose();

        Assert.Throws<ObjectDisposedException>(() => controller.CircuitState);
        Assert.Throws<ObjectDisposedException>(() => controller.LastException);
        Assert.Throws<ObjectDisposedException>(() => controller.LastHandledOutcome);

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.CloseCircuitAsync(context));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.IsolateCircuitAsync(context));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionPreExecuteAsync(context));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnUnhandledOutcomeAsync(Outcome.FromResult(10), context));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnHandledOutcomeAsync(Outcome.FromResult(10), context));
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByValue()
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromResult(99));
        var exception = (BrokenCircuitException)(await controller.OnActionPreExecuteAsync(context)).Value.Exception!;
        exception.RetryAfter.ShouldNotBeNull();
        exception.TelemetrySource.ShouldNotBeNull();

        GetBlockedTill(controller).ShouldBe(_timeProvider.GetUtcNow() + _options.BreakDuration);
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
                e.Message.ShouldBe("The circuit is now open and is not allowing calls.");
                e.RetryAfter.ShouldNotBeNull();
                e.TelemetrySource.ShouldNotBeNull();

                if (innerException)
                {
                    e.InnerException.ShouldBeOfType<InvalidOperationException>();
                }
                else
                {
                    e.InnerException.ShouldBeNull();
                }
            }
        }

        stacks.Distinct().Count().ShouldBe(1);
    }

    [Fact]
    public async Task HalfOpen_EnsureBreakDuration()
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen, context);
        GetBlockedTill(controller).ShouldBe(_timeProvider.GetUtcNow() + _options.BreakDuration);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HalfOpen_EnsureCorrectStateTransitionAfterExecution(bool success)
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen, context);

        if (success)
        {
            await controller.OnUnhandledOutcomeAsync(Outcome.FromResult(0), context);
            controller.CircuitState.ShouldBe(CircuitState.Closed);

            _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
            _circuitBehavior.Received().OnCircuitClosed();
        }
        else
        {
            await controller.OnHandledOutcomeAsync(Outcome.FromResult(0), context);
            controller.CircuitState.ShouldBe(CircuitState.Open);

            _circuitBehavior.DidNotReceiveWithAnyArgs().OnActionSuccess(default);
            _circuitBehavior.Received().OnActionFailure(CircuitState.HalfOpen, out _);
        }
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByException()
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromException<int>(new InvalidOperationException()));
        var exception = (BrokenCircuitException)(await controller.OnActionPreExecuteAsync(context)).Value.Exception!;
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.RetryAfter.ShouldNotBeNull();
        exception.TelemetrySource.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnActionFailure_EnsureLock()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

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
        var executeAction = Task.Run(() => controller.OnHandledOutcomeAsync(Outcome.FromResult(0), context));
        executing.WaitOne();
        var executeAction2 = Task.Run(() => controller.OnHandledOutcomeAsync(Outcome.FromResult(0), context));

        // assert
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        executeAction.Wait(50, cancellationToken).ShouldBeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
        verified.Set();
        await executeAction;
        await executeAction2;
    }

    [Fact]
    public async Task OnActionPreExecute_HalfOpen()
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
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
        await controller.OnActionPreExecuteAsync(context);
        var error = (await controller.OnActionPreExecuteAsync(context)).Value.Exception;

        // assert
        var exception = error.ShouldBeOfType<BrokenCircuitException>();
        exception.RetryAfter.ShouldNotBeNull();
        exception.TelemetrySource.ShouldNotBeNull();
        controller.CircuitState.ShouldBe(CircuitState.HalfOpen);
        called.ShouldBeTrue();
    }

    [InlineData(CircuitState.HalfOpen, CircuitState.Closed)]
    [InlineData(CircuitState.Isolated, CircuitState.Isolated)]
    [InlineData(CircuitState.Closed, CircuitState.Closed)]
    [Theory]
    public async Task OnActionSuccess_EnsureCorrectBehavior(CircuitState state, CircuitState expectedState)
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
        var called = false;
        _options.OnClosed = args =>
        {
            args.IsManual.ShouldBeFalse();
            called = true;
            return default;
        };

        using var controller = CreateController();

        await TransitionToState(controller, state, context);

        // act
        await controller.OnUnhandledOutcomeAsync(Outcome.FromResult(10), context);

        // assert
        controller.CircuitState.ShouldBe(expectedState);

        _circuitBehavior.Received().OnActionSuccess(state);
        if (expectedState == CircuitState.Closed && state != CircuitState.Closed)
        {
            _circuitBehavior.Received().OnCircuitClosed();
            called.ShouldBeTrue();
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
        var context = ResilienceContextPool.Shared.Get();
        var called = false;
        _options.OnOpened = args =>
        {
            if (state == CircuitState.Isolated)
            {
                args.IsManual.ShouldBeTrue();
            }
            else
            {
                args.IsManual.ShouldBeFalse();
            }

            called = true;
            return default;
        };
        using var controller = CreateController();

        await TransitionToState(controller, state, context);

        _circuitBehavior.When(x => x.OnActionFailure(state, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnHandledOutcomeAsync(Outcome.FromResult(99), context);

        // assert
        controller.LastHandledOutcome!.Value.Result.ShouldBe(99);
        controller.CircuitState.ShouldBe(expectedState);
        _circuitBehavior.Received().OnActionFailure(state, out Arg.Any<bool>());

        if (expectedState == CircuitState.Open && state != CircuitState.Open)
        {
            called.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task OnActionFailureAsync_EnsureBreakDurationGeneration()
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
        _options.BreakDurationGenerator = static args =>
        {
            args.FailureCount.ShouldBe(1);
            args.FailureRate.ShouldBe(0.5);
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(42));
        };

        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.Closed, context);

        var utcNow = new DateTimeOffset(2023, 12, 12, 12, 34, 56, TimeSpan.Zero);
        _timeProvider.SetUtcNow(utcNow);

        _circuitBehavior.FailureCount.Returns(1);
        _circuitBehavior.FailureRate.Returns(0.5);
        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
            .Do(x => x[1] = true);

        // act
        await controller.OnHandledOutcomeAsync(Outcome.FromResult(99), context);

        // assert
        var blockedTill = GetBlockedTill(controller);
        blockedTill.ShouldBe(utcNow + TimeSpan.FromMinutes(42));
    }

    [Fact]
    public async Task BreakDurationGenerator_EnsureHalfOpenAttempts()
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
        var halfOpenAttempts = new List<int>();

        _options.BreakDurationGenerator = args =>
        {
            halfOpenAttempts.Add(args.HalfOpenAttempts);
            return new ValueTask<TimeSpan>(TimeSpan.Zero);
        };

        using var controller = CreateController();

        // act
        await TransitionToState(controller, CircuitState.Closed, context);

        for (int i = 0; i < 5; i++)
        {
            await TransitionToState(controller, CircuitState.Open, context);
            await TransitionToState(controller, CircuitState.HalfOpen, context);
        }

        await TransitionToState(controller, CircuitState.Closed, context);

        for (int i = 0; i < 3; i++)
        {
            await TransitionToState(controller, CircuitState.Open, context);
            await TransitionToState(controller, CircuitState.HalfOpen, context);
        }

        // assert
        halfOpenAttempts.ShouldBe([0, 1, 2, 3, 4, 0, 1, 2]);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task OnActionFailureAsync_EnsureBreakDurationNotOverflow(bool overflow)
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();
        var shouldBreak = true;
        await TransitionToState(controller, CircuitState.HalfOpen, context);
        var utcNow = DateTimeOffset.MaxValue - _options.BreakDuration;
        if (overflow)
        {
            utcNow += TimeSpan.FromMilliseconds(10);
        }

        _timeProvider.SetUtcNow(utcNow);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.HalfOpen, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnHandledOutcomeAsync(Outcome.FromResult(99), context);

        // assert
        var blockedTill = GetBlockedTill(controller);

        if (overflow)
        {
            blockedTill.ShouldBe(DateTimeOffset.MaxValue);
        }
        else
        {
            blockedTill.ShouldBe(utcNow + _options.BreakDuration);
        }
    }

    [Fact]
    public async Task OnActionFailureAsync_VoidResult_EnsureBreakingExceptionNotSet()
    {
        // arrange
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();
        bool shouldBreak = true;
        await TransitionToState(controller, CircuitState.Open, context);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Open, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        // act
        await controller.OnHandledOutcomeAsync(Outcome.FromResult(99), context);

        // assert
        controller.LastException.ShouldBeNull();
        var outcome = await controller.OnActionPreExecuteAsync(context);
        var exception = outcome.Value.Exception.ShouldBeOfType<BrokenCircuitException>();
        exception.RetryAfter.ShouldNotBeNull();
        exception.TelemetrySource.ShouldNotBeNull();
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Closed()
    {
        var context = ResilienceContextPool.Shared.Get();
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen, context);

        await controller.OnUnhandledOutcomeAsync(Outcome.FromResult(0), context);
        controller.CircuitState.ShouldBe(CircuitState.Closed);

        _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
        _circuitBehavior.Received().OnCircuitClosed();
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Open_HalfOpen_Closed()
    {
        var cancellationToken = CancellationToken.None;
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        using var controller = CreateController();
        bool shouldBreak = true;

        await TransitionToState(controller, CircuitState.HalfOpen, context);

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.HalfOpen, out Arg.Any<bool>()))
                        .Do(x => x[1] = shouldBreak);

        await controller.OnHandledOutcomeAsync(Outcome.FromResult(0), context);
        controller.CircuitState.ShouldBe(CircuitState.Open);

        // execution rejected
        TimeSpan advanceTimeRejected = TimeSpan.FromMilliseconds(1);
        AdvanceTime(advanceTimeRejected);
        var outcome = await controller.OnActionPreExecuteAsync(context);
        var exception = outcome.Value.Exception.ShouldBeOfType<BrokenCircuitException>();
        exception.RetryAfter.ShouldBe(_options.BreakDuration - advanceTimeRejected);
        exception.TelemetrySource.ShouldNotBeNull();

        // wait and try, transition to half open
        AdvanceTime(_options.BreakDuration + _options.BreakDuration);
        await controller.OnActionPreExecuteAsync(context);
        controller.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // close circuit
        await controller.OnUnhandledOutcomeAsync(Outcome.FromResult(0), context);
        controller.CircuitState.ShouldBe(CircuitState.Closed);

        _circuitBehavior.Received().OnActionSuccess(CircuitState.HalfOpen);
        _circuitBehavior.Received().OnCircuitClosed();
    }

    [Fact]
    public async Task ExecuteScheduledTask_Async_Ok()
    {
        var cancellationToken = CancellationToken.None;
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        var source = new TaskCompletionSource<string>();
        var task = CircuitStateController<string>.ExecuteScheduledTaskAsync(source.Task, context.Initialize<string>(isSynchronous: false)).AsTask();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        task.Wait(3, cancellationToken).ShouldBeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
        task.IsCompleted.ShouldBeFalse();

        source.SetResult("ok");

        await task;
        task.IsCompleted.ShouldBeTrue();
    }

    private static DateTimeOffset? GetBlockedTill(CircuitStateController<int> controller) =>
        (DateTimeOffset?)controller.GetType().GetField("_blockedUntil", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)!;

    private async Task TransitionToState(
        CircuitStateController<int> controller,
        CircuitState state,
        ResilienceContext context)
    {
        switch (state)
        {
            case CircuitState.Closed:
                await controller.CloseCircuitAsync(context);
                break;
            case CircuitState.Open:
                await OpenCircuit(controller);
                break;
            case CircuitState.HalfOpen:
                await OpenCircuit(controller);
                AdvanceTime(_options.BreakDuration);
                await controller.OnActionPreExecuteAsync(context);
                break;
            case CircuitState.Isolated:
                await controller.IsolateCircuitAsync(context);
                break;
        }

        controller.CircuitState.ShouldBe(state);
    }

    private async Task OpenCircuit(CircuitStateController<int> controller, Outcome<int>? outcome = null)
    {
        bool breakCircuit = true;

        _circuitBehavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
                        .Do(x => x[1] = breakCircuit);

        await controller.OnHandledOutcomeAsync(
            outcome ?? Outcome.FromResult(10),
            ResilienceContextPool.Shared.Get().Initialize<int>(true));
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
