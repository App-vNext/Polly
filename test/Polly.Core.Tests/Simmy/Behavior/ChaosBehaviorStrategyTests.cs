using Polly.Simmy.Behavior;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Behavior;

public class ChaosBehaviorStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly ChaosBehaviorStrategyOptions _options;
    private readonly List<TelemetryEventArguments<object, object>> _args = [];
    private bool _behaviorGeneratorExecuted;
    private bool _onBehaviorInjectedExecuted;
    private bool _userDelegateExecuted;

    public ChaosBehaviorStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));
        _options = new();
        _userDelegateExecuted = false;
        _behaviorGeneratorExecuted = false;
        _onBehaviorInjectedExecuted = false;
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_behavior()
    {
        _options.InjectionRate = 0.6;
        _options.Enabled = false;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };

        var sut = CreateSut();
        sut.Execute(() => { _userDelegateExecuted = true; });

        _userDelegateExecuted.ShouldBeTrue();
        _behaviorGeneratorExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_behavior()
    {
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; }, TestCancellation.Token);

        _userDelegateExecuted.ShouldBeTrue();
        _behaviorGeneratorExecuted.ShouldBeTrue();
        _onBehaviorInjectedExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_ensure_on_behavior_injected_called()
    {
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };
        _options.OnBehaviorInjected = args =>
        {
            args.Context.ShouldNotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.ShouldBeFalse();
            _onBehaviorInjectedExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; }, TestCancellation.Token);

        _onBehaviorInjectedExecuted.ShouldBeTrue();
        _userDelegateExecuted.ShouldBeTrue();
        _behaviorGeneratorExecuted.ShouldBeTrue();
        _args.Count.ShouldBe(1);
        _args[0].Arguments.ShouldBeOfType<OnBehaviorInjectedArguments>();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_not_within_threshold_should_not_inject_behavior()
    {
        _options.InjectionRate = 0.4;
        _options.Enabled = false;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; }, TestCancellation.Token);

        _userDelegateExecuted.ShouldBeTrue();
        _behaviorGeneratorExecuted.ShouldBeFalse();
        _onBehaviorInjectedExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_inject_behavior_before_executing_user_delegate()
    {
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) =>
        {
            _userDelegateExecuted.ShouldBeFalse(); // Not yet executed at the time the injected behavior runs.
            _behaviorGeneratorExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; }, TestCancellation.Token);

        _userDelegateExecuted.ShouldBeTrue();
        _behaviorGeneratorExecuted.ShouldBeTrue();
        _onBehaviorInjectedExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_injected_behavior()
    {
        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) =>
        {
            cts.Cancel();
            _behaviorGeneratorExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await Should.ThrowAsync<OperationCanceledException>(() => sut.ExecuteAsync(async _ => await Task.CompletedTask, cts.Token).AsTask());

        _userDelegateExecuted.ShouldBeFalse();
        _behaviorGeneratorExecuted.ShouldBeTrue();
        _onBehaviorInjectedExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        bool enabledGeneratorExecuted = false;

        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.EnabledGenerator = (_) =>
        {
            cts.Cancel();
            enabledGeneratorExecuted = true;
            return new ValueTask<bool>(true);
        };

        var sut = CreateSut();
        await Should.ThrowAsync<OperationCanceledException>(() => sut.ExecuteAsync(async _ => await Task.CompletedTask, cts.Token).AsTask());

        _userDelegateExecuted.ShouldBeFalse();
        enabledGeneratorExecuted.ShouldBeTrue();
        _onBehaviorInjectedExecuted.ShouldBeFalse();
    }

    private ResiliencePipeline CreateSut() => new ChaosBehaviorStrategy(_options, _telemetry).AsPipeline();
}
