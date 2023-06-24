using Moq;
using Polly.CircuitBreaker;
using Polly.Telemetry;

namespace Polly.Core.Tests.CircuitBreaker.Controller;
public class CircuitStateControllerTests
{
    private readonly MockTimeProvider _timeProvider = new();
    private readonly CircuitBreakerStrategyOptions<int> _options = new SimpleCircuitBreakerStrategyOptions<int>();
    private readonly Mock<CircuitBehavior> _circuitBehavior = new(MockBehavior.Strict);
    private readonly Action<TelemetryEventArguments> _onTelemetry = _ => { };
    private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

    public CircuitStateControllerTests() => _timeProvider.Setup(v => v.GetUtcNow()).Returns(() => _utcNow);

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

        _timeProvider.Setup(v => v.GetUtcNow()).Returns(DateTime.UtcNow);
        using var controller = CreateController();
        var context = ResilienceContext.Get();

        // act
        await controller.IsolateCircuitAsync(context);

        // assert
        controller.CircuitState.Should().Be(CircuitState.Isolated);
        called.Should().BeTrue();

        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        outcome.Value.Exception.Should().BeOfType<IsolatedCircuitException>();

        // now close it
        _circuitBehavior.Setup(v => v.OnCircuitClosed());
        await controller.CloseCircuitAsync(ResilienceContext.Get());
        await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        context.ResilienceEvents.Should().Contain(new ResilienceEvent("OnCircuitOpened"));
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

        _timeProvider.Setup(v => v.GetUtcNow()).Returns(DateTime.UtcNow);
        using var controller = CreateController();
        await controller.IsolateCircuitAsync(ResilienceContext.Get());
        _circuitBehavior.Setup(v => v.OnCircuitClosed());
        var context = ResilienceContext.Get();

        // act
        await controller.CloseCircuitAsync(context);

        // assert
        called.Should().BeTrue();

        await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        _circuitBehavior.VerifyAll();
        context.ResilienceEvents.Should().Contain(new ResilienceEvent("OnCircuitClosed"));
    }

    [Fact]
    public async Task Disposed_EnsureThrows()
    {
        var controller = CreateController();
        controller.Dispose();

        Assert.Throws<ObjectDisposedException>(() => controller.CircuitState);
        Assert.Throws<ObjectDisposedException>(() => controller.LastException);
        Assert.Throws<ObjectDisposedException>(() => controller.LastHandledOutcome);

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.CloseCircuitAsync(ResilienceContext.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.IsolateCircuitAsync(ResilienceContext.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionSuccessAsync(Outcome.FromResult(10), ResilienceContext.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionFailureAsync(Outcome.FromResult(10), ResilienceContext.Get()));
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByValue()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromResult(99));
        var error = (BrokenCircuitException<int>)(await controller.OnActionPreExecuteAsync(ResilienceContext.Get())).Value.Exception!;
        error.Should().BeOfType<BrokenCircuitException<int>>();
        error.Result.Should().Be(99);

        GetBlockedTill(controller).Should().Be(_utcNow + _options.BreakDuration);
    }

    [Fact]
    public async Task HalfOpen_EnsureBreakDuration()
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);
        GetBlockedTill(controller).Should().Be(_utcNow + _options.BreakDuration);
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
            _circuitBehavior.Setup(v => v.OnActionSuccess(CircuitState.HalfOpen));
            _circuitBehavior.Setup(v => v.OnCircuitClosed());

            await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContext.Get());
            controller.CircuitState.Should().Be(CircuitState.Closed);
        }
        else
        {
            var shouldBreak = true;
            _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.HalfOpen, out shouldBreak));
            await controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContext.Get());
            controller.CircuitState.Should().Be(CircuitState.Open);
        }
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByException()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, Outcome.FromException<int>(new InvalidOperationException()));
        var error = (BrokenCircuitException)(await controller.OnActionPreExecuteAsync(ResilienceContext.Get())).Value.Exception!;
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
        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.Closed, out shouldBreak)).Callback(() =>
        {
            executing.Set();
            verified.WaitOne();
        });

        using var controller = CreateController();

        // act
        var executeAction = Task.Run(() => controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContext.Get()));
        executing.WaitOne();
        var executeAction2 = Task.Run(() => controller.OnActionFailureAsync(Outcome.FromResult(0), ResilienceContext.Get()));

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
        await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        var error = (await controller.OnActionPreExecuteAsync(ResilienceContext.Get())).Value.Exception;
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

        _circuitBehavior.Setup(v => v.OnActionSuccess(state));
        if (expectedState == CircuitState.Closed && state != CircuitState.Closed)
        {
            _circuitBehavior.Setup(v => v.OnCircuitClosed());
        }

        // act
        await controller.OnActionSuccessAsync(Outcome.FromResult(10), ResilienceContext.Get());

        // assert
        controller.CircuitState.Should().Be(expectedState);
        _circuitBehavior.VerifyAll();

        if (expectedState == CircuitState.Closed && state != CircuitState.Closed)
        {
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
        _circuitBehavior.Setup(v => v.OnActionFailure(state, out shouldBreak));

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContext.Get());

        // assert
        controller.LastHandledOutcome!.Value.Result.Should().Be(99);
        controller.CircuitState.Should().Be(expectedState);
        _circuitBehavior.VerifyAll();

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
        _utcNow = DateTime.MaxValue - _options.BreakDuration;
        if (overflow)
        {
            _utcNow += TimeSpan.FromMilliseconds(10);
        }

        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.HalfOpen, out shouldBreak));

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContext.Get());

        // assert
        var blockedTill = GetBlockedTill(controller);

        if (overflow)
        {
            blockedTill.Should().Be(DateTimeOffset.MaxValue);
        }
        else
        {
            blockedTill.Should().Be(_utcNow + _options.BreakDuration);
        }
    }

    [Fact]
    public async Task OnActionFailureAsync_VoidResult_EnsureBreakingExceptionNotSet()
    {
        // arrange
        using var controller = CreateController();
        bool shouldBreak = true;
        await TransitionToState(controller, CircuitState.Open);
        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.Open, out shouldBreak));

        // act
        await controller.OnActionFailureAsync(Outcome.FromResult(99), ResilienceContext.Get());

        // assert
        controller.LastException.Should().BeNull();
        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException<int>>();
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Closed()
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);
        _circuitBehavior.Setup(v => v.OnActionSuccess(CircuitState.HalfOpen));
        _circuitBehavior.Setup(v => v.OnCircuitClosed());

        await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContext.Get());
        controller.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Open_HalfOpen_Closed()
    {
        var context = ResilienceContext.Get();
        using var controller = CreateController();
        bool shouldBreak = true;

        await TransitionToState(controller, CircuitState.HalfOpen);

        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.HalfOpen, out shouldBreak));
        await controller.OnActionFailureAsync(Outcome.FromResult(0), context);
        controller.CircuitState.Should().Be(CircuitState.Open);

        // execution rejected
        AdvanceTime(TimeSpan.FromMilliseconds(1));
        var outcome = await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        outcome.Value.Exception.Should().BeOfType<BrokenCircuitException<int>>();

        // wait and try, transition to half open
        AdvanceTime(_options.BreakDuration + _options.BreakDuration);
        await controller.OnActionPreExecuteAsync(context);
        controller.CircuitState.Should().Be(CircuitState.HalfOpen);

        // close circuit
        _circuitBehavior.Setup(v => v.OnActionSuccess(CircuitState.HalfOpen));
        _circuitBehavior.Setup(v => v.OnCircuitClosed());
        await controller.OnActionSuccessAsync(Outcome.FromResult(0), ResilienceContext.Get());
        controller.CircuitState.Should().Be(CircuitState.Closed);
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
                await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
                break;
            case CircuitState.Isolated:
                await controller.IsolateCircuitAsync(ResilienceContext.Get());
                break;
        }

        controller.CircuitState.Should().Be(state);
    }

    private async Task OpenCircuit(CircuitStateController<int> controller, Outcome<int>? outcome = null)
    {
        bool breakCircuit = true;
        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.Closed, out breakCircuit));
        await controller.OnActionFailureAsync(outcome ?? Outcome.FromResult(10), ResilienceContext.Get().Initialize<int>(true));
    }

    private void AdvanceTime(TimeSpan timespan) => _utcNow += timespan;

    private CircuitStateController<int> CreateController() => new(
        _options.BreakDuration,
        _options.OnOpened,
        _options.OnClosed,
        _options.OnHalfOpened,
        _circuitBehavior.Object,
        _timeProvider.Object,
        TestUtilities.CreateResilienceTelemetry(args => _onTelemetry.Invoke(args)));
}
