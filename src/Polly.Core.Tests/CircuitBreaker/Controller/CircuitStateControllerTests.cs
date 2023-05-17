using System;
using System.Threading.Tasks;
using Moq;
using Polly.CircuitBreaker;
using Polly.Strategy;

namespace Polly.Core.Tests.CircuitBreaker.Controller;
public class CircuitStateControllerTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CircuitBreakerStrategyOptions _options = new SimpleCircuitBreakerStrategyOptions();
    private readonly Mock<CircuitBehavior> _circuitBehavior = new(MockBehavior.Strict);
    private readonly Action<IResilienceArguments> _onTelemetry = _ => { };
    private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

    public CircuitStateControllerTests() => _timeProvider.Setup(v => v.UtcNow).Returns(() => _utcNow);

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
        _options.OnOpened.RegisterVoid((outcome, args) =>
        {
            args.BreakDuration.Should().Be(TimeSpan.MaxValue);
            args.Context.IsSynchronous.Should().BeFalse();
            args.Context.IsVoid.Should().BeTrue();
            args.IsManual.Should().BeTrue();
            outcome.IsVoidResult.Should().BeTrue();
            called = true;
        });

        _timeProvider.Setup(v => v.UtcNow).Returns(DateTime.UtcNow);
        using var controller = CreateController();
        var context = ResilienceContext.Get();

        // act
        await controller.IsolateCircuitAsync(context);

        // assert
        controller.CircuitState.Should().Be(CircuitState.Isolated);
        called.Should().BeTrue();

        await Assert.ThrowsAsync<IsolatedCircuitException>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));

        // now close it
        _circuitBehavior.Setup(v => v.OnCircuitClosed());
        await controller.CloseCircuitAsync(ResilienceContext.Get());
        await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        context.ResilienceEvents.Should().Contain(new ReportedResilienceEvent("OnCircuitOpened"));
    }

    [Fact]
    public async Task BreakAsync_Ok()
    {
        // arrange
        bool called = false;
        _options.OnClosed.RegisterVoid((outcome, args) =>
        {
            args.Context.IsSynchronous.Should().BeFalse();
            args.Context.IsVoid.Should().BeTrue();
            args.IsManual.Should().BeTrue();
            outcome.IsVoidResult.Should().BeTrue();
            called = true;
        });

        _timeProvider.Setup(v => v.UtcNow).Returns(DateTime.UtcNow);
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
        context.ResilienceEvents.Should().Contain(new ReportedResilienceEvent("OnCircuitClosed"));
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
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionSuccessAsync(new Outcome<int>(10), ResilienceContext.Get()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.OnActionFailureAsync(new Outcome<int>(10), ResilienceContext.Get()));
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByValue()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, new Outcome<int>(99));
        var error = await Assert.ThrowsAsync<BrokenCircuitException<int>>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));
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

            await controller.OnActionSuccessAsync(new Outcome<int>(0), ResilienceContext.Get());
            controller.CircuitState.Should().Be(CircuitState.Closed);
        }
        else
        {
            var shouldBreak = true;
            _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.HalfOpen, out shouldBreak));
            await controller.OnActionFailureAsync(new Outcome<int>(0), ResilienceContext.Get());
            controller.CircuitState.Should().Be(CircuitState.Open);
        }
    }

    [Fact]
    public async Task OnActionPreExecute_CircuitOpenedByException()
    {
        using var controller = CreateController();

        await OpenCircuit(controller, new Outcome<int>(new InvalidOperationException()));
        var error = await Assert.ThrowsAsync<BrokenCircuitException>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));
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
        var executeAction = Task.Run(() => controller.OnActionFailureAsync(new Outcome<int>(0), ResilienceContext.Get()));
        executing.WaitOne();
        var executeAction2 = Task.Run(() => controller.OnActionFailureAsync(new Outcome<int>(0), ResilienceContext.Get()));

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
        _options.OnHalfOpened.Register(_ => called = true);
        using var controller = CreateController();

        await OpenCircuit(controller, new Outcome<int>(10));
        AdvanceTime(_options.BreakDuration);

        // act
        await controller.OnActionPreExecuteAsync(ResilienceContext.Get());
        var error = await Assert.ThrowsAsync<BrokenCircuitException<int>>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));

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
        _options.OnClosed.Register<int>((_, args) =>
        {
            args.IsManual.Should().BeFalse();
            called = true;
        });
        using var controller = CreateController();

        await TransitionToState(controller, state);

        _circuitBehavior.Setup(v => v.OnActionSuccess(state));
        if (expectedState == CircuitState.Closed && state != CircuitState.Closed)
        {
            _circuitBehavior.Setup(v => v.OnCircuitClosed());
        }

        // act
        await controller.OnActionSuccessAsync(new Outcome<int>(10), ResilienceContext.Get());

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
        _options.OnOpened.Register<string>((_, args) =>
        {
            args.IsManual.Should().BeFalse();
            called = true;
        });
        using var controller = CreateController();

        await TransitionToState(controller, state);
        _circuitBehavior.Setup(v => v.OnActionFailure(state, out shouldBreak));

        // act
        await controller.OnActionFailureAsync(new Outcome<string>("dummy"), ResilienceContext.Get());

        // assert
        controller.LastHandledOutcome!.Value.Result.Should().Be("dummy");
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
        await controller.OnActionFailureAsync(new Outcome<string>("dummy"), ResilienceContext.Get());

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
        await controller.OnActionFailureAsync(new Outcome<VoidResult>(VoidResult.Instance), ResilienceContext.Get());

        // assert
        controller.LastException.Should().BeNull();
        await Assert.ThrowsAsync<BrokenCircuitException>(async () => await controller.OnActionPreExecuteAsync(ResilienceContext.Get()));
    }

    [Fact]
    public async Task Flow_Closed_HalfOpen_Closed()
    {
        using var controller = CreateController();

        await TransitionToState(controller, CircuitState.HalfOpen);
        _circuitBehavior.Setup(v => v.OnActionSuccess(CircuitState.HalfOpen));
        _circuitBehavior.Setup(v => v.OnCircuitClosed());

        await controller.OnActionSuccessAsync(new Outcome<int>(0), ResilienceContext.Get());
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
        await controller.OnActionFailureAsync(new Outcome<int>(0), context);
        controller.CircuitState.Should().Be(CircuitState.Open);

        // execution rejected
        AdvanceTime(TimeSpan.FromMilliseconds(1));
        await Assert.ThrowsAsync<BrokenCircuitException<int>>(async () => await controller.OnActionPreExecuteAsync(context));

        // wait and try, transition to half open
        AdvanceTime(_options.BreakDuration + _options.BreakDuration);
        await controller.OnActionPreExecuteAsync(context);
        controller.CircuitState.Should().Be(CircuitState.HalfOpen);

        // close circuit
        _circuitBehavior.Setup(v => v.OnActionSuccess(CircuitState.HalfOpen));
        _circuitBehavior.Setup(v => v.OnCircuitClosed());
        await controller.OnActionSuccessAsync(new Outcome<int>(0), ResilienceContext.Get());
        controller.CircuitState.Should().Be(CircuitState.Closed);
    }

    private static DateTimeOffset? GetBlockedTill(CircuitStateController controller) =>
        (DateTimeOffset?)controller.GetType().GetField("_blockedUntil", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)!;

    private async Task TransitionToState(CircuitStateController controller, CircuitState state)
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

    private async Task OpenCircuit(CircuitStateController controller, Outcome<int>? outcome = null)
    {
        bool breakCircuit = true;
        _circuitBehavior.Setup(v => v.OnActionFailure(CircuitState.Closed, out breakCircuit));
        await controller.OnActionFailureAsync(outcome ?? new Outcome<int>(10), ResilienceContext.Get().Initialize<int>(true));
    }

    private void AdvanceTime(TimeSpan timespan) => _utcNow += timespan;

    private CircuitStateController CreateController() => new(
        _options,
        _circuitBehavior.Object,
        _timeProvider.Object,
        TestUtilities.CreateResilienceTelemetry(args => _onTelemetry.Invoke(args)));
}
