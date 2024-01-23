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

        _userDelegateExecuted.Should().BeTrue();
        _behaviorGeneratorExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_behavior()
    {
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; });

        _userDelegateExecuted.Should().BeTrue();
        _behaviorGeneratorExecuted.Should().BeTrue();
        _onBehaviorInjectedExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_ensure_on_behavior_injected_called()
    {
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };
        _options.OnBehaviorInjected = args =>
        {
            args.Context.Should().NotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
            _onBehaviorInjectedExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; });

        _onBehaviorInjectedExecuted.Should().BeTrue();
        _userDelegateExecuted.Should().BeTrue();
        _behaviorGeneratorExecuted.Should().BeTrue();
        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnBehaviorInjectedArguments>();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_not_within_threshold_should_not_inject_behavior()
    {
        _options.InjectionRate = 0.4;
        _options.Enabled = false;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) => { _behaviorGeneratorExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; });

        _userDelegateExecuted.Should().BeTrue();
        _behaviorGeneratorExecuted.Should().BeFalse();
        _onBehaviorInjectedExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_inject_behavior_before_executing_user_delegate()
    {
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) =>
        {
            _userDelegateExecuted.Should().BeFalse(); // Not yet executed at the time the injected behavior runs.
            _behaviorGeneratorExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { _userDelegateExecuted = true; return default; });

        _userDelegateExecuted.Should().BeTrue();
        _behaviorGeneratorExecuted.Should().BeTrue();
        _onBehaviorInjectedExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_injected_behavior()
    {
        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorGenerator = (_) =>
        {
            cts.Cancel();
            _behaviorGeneratorExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.Invoking(s => s.ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        _userDelegateExecuted.Should().BeFalse();
        _behaviorGeneratorExecuted.Should().BeTrue();
        _onBehaviorInjectedExecuted.Should().BeFalse();
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
        await sut.Invoking(s => s.ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        _userDelegateExecuted.Should().BeFalse();
        enabledGeneratorExecuted.Should().BeTrue();
        _onBehaviorInjectedExecuted.Should().BeFalse();
    }

    private ResiliencePipeline CreateSut() => new ChaosBehaviorStrategy(_options, _telemetry).AsPipeline();
}
