using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Polly.CircuitBreaker;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResilienceStrategyTests : IDisposable
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly CircuitBehavior _behavior;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly CircuitBreakerStrategyOptions<int> _options;
    private readonly CircuitStateController<int> _controller;

    public CircuitBreakerResilienceStrategyTests()
    {
        _timeProvider = new FakeTimeProvider();
        _behavior = Substitute.For<CircuitBehavior>();
        _telemetry = TestUtilities.CreateResilienceTelemetry(_ => { });
        _options = new CircuitBreakerStrategyOptions<int>();
        _controller = new CircuitStateController<int>(
            CircuitBreakerConstants.DefaultBreakDuration,
            null,
            null,
            null,
            _behavior,
            _timeProvider,
            _telemetry);
    }

    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => Create()).Should().NotThrow();
    }

    [Fact]
    public void Ctor_StateProvider_EnsureAttached()
    {
        _options.StateProvider = new CircuitBreakerStateProvider();
        Create();

        _options.StateProvider.IsInitialized.Should().BeTrue();

        _options.StateProvider.CircuitState.Should().Be(CircuitState.Closed);
        _options.StateProvider.LastHandledOutcome.Should().Be(null);
    }

    [Fact]
    public async Task Ctor_ManualControl_EnsureAttached()
    {
        _options.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
        _options.ManualControl = new CircuitBreakerManualControl();
        var strategy = Create();

        await _options.ManualControl.IsolateAsync(CancellationToken.None);
        strategy.Invoking(s => s.Execute(_ => 0)).Should().Throw<IsolatedCircuitException>();

        await _options.ManualControl.CloseAsync(CancellationToken.None);

        strategy.Invoking(s => s.Execute(_ => 0)).Should().NotThrow();

        _options.ManualControl.Dispose();
        strategy.Invoking(s => s.Execute(_ => 0)).Should().Throw<ObjectDisposedException>();

        _behavior.Received().OnCircuitClosed();
        _behavior.Received().OnActionSuccess(CircuitState.Closed);
    }

    [Fact]
    public void Execute_HandledResult_OnFailureCalled()
    {
        _options.ShouldHandle = args => new ValueTask<bool>(args.Result is -1);
        var strategy = Create();
        var shouldBreak = false;

        _behavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
                 .Do(x => x[1] = shouldBreak);

        strategy.Execute(_ => -1).Should().Be(-1);

        _behavior.Received().OnActionFailure(CircuitState.Closed, out Arg.Any<bool>());
    }

    [Fact]
    public void Execute_UnhandledResult_OnActionSuccess()
    {
        _options.ShouldHandle = args => new ValueTask<bool>(args.Result is -1);
        var strategy = Create();

        strategy.Execute(_ => 0).Should().Be(0);

        _behavior.Received(1).OnActionSuccess(CircuitState.Closed);
    }

    [Fact]
    public void Execute_HandledException_OnFailureCalled()
    {
        _options.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
        var strategy = Create();
        var shouldBreak = false;

        _behavior.When(v => v.OnActionFailure(CircuitState.Closed, out Arg.Any<bool>()))
                 .Do(x => x[1] = shouldBreak);

        strategy.Invoking(s => s.Execute<int>(_ => throw new InvalidOperationException())).Should().Throw<InvalidOperationException>();

        _behavior.Received().OnActionFailure(CircuitState.Closed, out Arg.Any<bool>());
    }

    [Fact]
    public void Execute_UnhandledException_NoCalls()
    {
        _options.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
        var strategy = Create();

        strategy.Invoking(s => s.Execute(_ => throw new ArgumentException())).Should().Throw<ArgumentException>();

        _behavior.DidNotReceiveWithAnyArgs().OnActionFailure(default, out Arg.Any<bool>());
        _behavior.DidNotReceiveWithAnyArgs().OnActionSuccess(default);
        _behavior.DidNotReceiveWithAnyArgs().OnCircuitClosed();
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void Execute_Ok()
    {
        _options.ShouldHandle = _ => PredicateResult.False;

        Create().Invoking(s => s.Execute(_ => 0)).Should().NotThrow();

        _behavior.Received(1).OnActionSuccess(CircuitState.Closed);
    }

    private ResiliencePipelineBridge<int> Create()
        => new(new CircuitBreakerResilienceStrategy<int>(_options.ShouldHandle!, _controller, _options.StateProvider, _options.ManualControl));
}
