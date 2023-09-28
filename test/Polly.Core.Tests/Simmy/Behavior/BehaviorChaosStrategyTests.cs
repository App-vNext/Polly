using Polly.Simmy.Behavior;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorChaosStrategyTests
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly BehaviorStrategyOptions _options;
    private readonly List<TelemetryEventArguments<object, object>> _args = new();

    public BehaviorChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));
        _options = new();
    }

    [Fact]
    public void Given_not_enabled_should_not_inject_behaviour()
    {
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = false;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) => { injectedBehaviourExecuted = true; return default; };

        var sut = CreateSut();
        sut.Execute(() => { userDelegateExecuted = true; });

        userDelegateExecuted.Should().BeTrue();
        injectedBehaviourExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_behaviour()
    {
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) => { injectedBehaviourExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { userDelegateExecuted = true; return default; });

        userDelegateExecuted.Should().BeTrue();
        injectedBehaviourExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_ensure_on_behavior_injected_called()
    {
        var called = false;
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) => { injectedBehaviourExecuted = true; return default; };
        _options.OnBehaviorInjected = args =>
        {
            args.Context.Should().NotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
            called = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { userDelegateExecuted = true; return default; });

        called.Should().BeTrue();
        userDelegateExecuted.Should().BeTrue();
        injectedBehaviourExecuted.Should().BeTrue();
        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnBehaviorInjectedArguments>();
    }

    [Fact]
    public async Task Given_enabled_and_randomly_not_within_threshold_should_not_inject_behaviour()
    {
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        _options.InjectionRate = 0.4;
        _options.Enabled = false;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) => { injectedBehaviourExecuted = true; return default; };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { userDelegateExecuted = true; return default; });

        userDelegateExecuted.Should().BeTrue();
        injectedBehaviourExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_inject_behaviour_before_executing_user_delegate()
    {
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) =>
        {
            userDelegateExecuted.Should().BeFalse(); // Not yet executed at the time the injected behaviour runs.
            injectedBehaviourExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.ExecuteAsync((_) => { userDelegateExecuted = true; return default; });

        userDelegateExecuted.Should().BeTrue();
        injectedBehaviourExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_injected_behavior()
    {
        var userDelegateExecuted = false;
        var injectedBehaviourExecuted = false;

        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.BehaviorAction = (_) =>
        {
            cts.Cancel();
            injectedBehaviourExecuted = true;
            return default;
        };

        var sut = CreateSut();
        await sut.Invoking(s => s.ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        userDelegateExecuted.Should().BeFalse();
        injectedBehaviourExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        var userDelegateExecuted = false;
        var enabledGeneratorExecuted = false;

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

        userDelegateExecuted.Should().BeFalse();
        enabledGeneratorExecuted.Should().BeTrue();
    }

    private ResiliencePipeline CreateSut() => new BehaviorChaosStrategy(_options, _telemetry).AsPipeline();
}
