using Microsoft.Extensions.Time.Testing;
using Polly.Simmy.Latency;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyStrategyTests : IDisposable
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ChaosLatencyStrategyOptions _options;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(500);
    private readonly List<TelemetryEventArguments<object, object>> _args = [];
    private bool _onLatencyInjectedExecuted;
    private bool _userDelegateExecuted;

    public ChaosLatencyStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));
        _options = new ChaosLatencyStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
        _onLatencyInjectedExecuted = false;
        _userDelegateExecuted = false;
    }

    public void Dispose() => _cancellationSource.Dispose();

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_latency()
    {
        _options.InjectionRate = 0.6;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;
        _options.OnLatencyInjected = args =>
        {
            args.Context.ShouldNotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.ShouldBeFalse();
            _onLatencyInjectedExecuted = true;
            return default;
        };

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { _userDelegateExecuted = true; await Task.CompletedTask; }, TestCancellation.Token);
        _timeProvider.Advance(_delay);
        await task;

        _userDelegateExecuted.ShouldBeTrue();
        _onLatencyInjectedExecuted.ShouldBeTrue();
        var after = _timeProvider.GetUtcNow();
        (after - before).ShouldBe(_delay);

        _args.Count.ShouldBe(1);
        _args[0].Arguments.ShouldBeOfType<OnLatencyInjectedArguments>();
    }

    [Fact]
    public async Task Given_not_enabled_should_not_inject_latency()
    {
        _options.InjectionRate = 0.6;
        _options.Enabled = false;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { _userDelegateExecuted = true; await Task.CompletedTask; }, TestCancellation.Token);
        _timeProvider.Advance(_delay);
        await task;

        _userDelegateExecuted.ShouldBeTrue();
        _onLatencyInjectedExecuted.ShouldBeFalse();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.ShouldBe(0);
    }

    [Fact]
    public async Task Given_enabled_and_randomly_not_within_threshold_should_not_inject_latency()
    {
        _options.InjectionRate = 0.4;
        _options.Enabled = false;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { _userDelegateExecuted = true; await Task.CompletedTask; }, TestCancellation.Token);
        _timeProvider.Advance(_delay);
        await task;

        _userDelegateExecuted.ShouldBeTrue();
        _onLatencyInjectedExecuted.ShouldBeFalse();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.ShouldBe(0);
    }

    [InlineData(-1000)]
    [InlineData(0)]
    [Theory]
    public async Task Given_latency_is_negative_should_not_inject_latency(double latency)
    {
        _options.InjectionRate = 0.6;
        _options.Latency = TimeSpan.FromSeconds(latency);
        _options.Randomizer = () => 0.5;

        _options.OnLatencyInjected = args =>
        {
            args.Context.ShouldNotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.ShouldBeFalse();
            _onLatencyInjectedExecuted = true;
            return default;
        };

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { _userDelegateExecuted = true; await Task.CompletedTask; }, TestCancellation.Token);
        _timeProvider.Advance(_delay);
        await task;

        _userDelegateExecuted.ShouldBeTrue();
        _onLatencyInjectedExecuted.ShouldBeFalse();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.ShouldBe(0);
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Randomizer = () => 0.5;
        _options.LatencyGenerator = (_) =>
        {
            cts.Cancel();
            return new ValueTask<TimeSpan>(_delay);
        };

        var sut = CreateSut();
        await Should.ThrowAsync<OperationCanceledException>(() => sut.ExecuteAsync(async _ => await Task.CompletedTask, cts.Token).AsTask());

        _userDelegateExecuted.ShouldBeFalse();
        _onLatencyInjectedExecuted.ShouldBeFalse();
    }

    private ResiliencePipeline CreateSut() => new ChaosLatencyStrategy(_options, _timeProvider, _telemetry).AsPipeline();
}
