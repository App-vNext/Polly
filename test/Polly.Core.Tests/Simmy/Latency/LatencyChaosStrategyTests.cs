using Microsoft.Extensions.Time.Testing;
using Polly.Simmy.Latency;
using Polly.Telemetry;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyChaosStrategyTests : IDisposable
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly LatencyStrategyOptions _options;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(500);
    private readonly List<TelemetryEventArguments<object, object>> _args = [];

    public LatencyChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));
        _options = new LatencyStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
    }

    public void Dispose() => _cancellationSource.Dispose();

    [Fact]
    public async Task Given_enabled_and_randomly_within_threshold_should_inject_latency()
    {
        var userDelegateExecuted = false;
        var onLatencyExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;
        _options.OnLatencyInjected = args =>
        {
            args.Context.Should().NotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
            onLatencyExecuted = true;
            return default;
        };

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { userDelegateExecuted = true; await Task.CompletedTask; });
        _timeProvider.Advance(_delay);
        await task;

        userDelegateExecuted.Should().BeTrue();
        var after = _timeProvider.GetUtcNow();
        (after - before).Should().Be(_delay);

        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnLatencyInjectedArguments>();
        onLatencyExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Given_not_enabled_should_not_inject_latency()
    {
        var userDelegateExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = false;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { userDelegateExecuted = true; await Task.CompletedTask; });
        _timeProvider.Advance(_delay);
        await task;

        userDelegateExecuted.Should().BeTrue();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.Should().Be(0);
    }

    [Fact]
    public async Task Given_enabled_and_randomly_not_within_threshold_should_not_inject_latency()
    {
        var userDelegateExecuted = false;

        _options.InjectionRate = 0.4;
        _options.Enabled = false;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { userDelegateExecuted = true; await Task.CompletedTask; });
        _timeProvider.Advance(_delay);
        await task;

        userDelegateExecuted.Should().BeTrue();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.Should().Be(0);
    }

    [InlineData(-1000)]
    [InlineData(0)]
    [Theory]
    public async Task Given_latency_is_negative_should_not_inject_latency(double latency)
    {
        var onLatencyExecuted = false;
        var userDelegateExecuted = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Latency = TimeSpan.FromSeconds(latency);
        _options.Randomizer = () => 0.5;

        _options.OnLatencyInjected = args =>
        {
            args.Context.Should().NotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
            onLatencyExecuted = true;
            return default;
        };

        var before = _timeProvider.GetUtcNow();
        var sut = CreateSut();
        var task = sut.ExecuteAsync(async _ => { userDelegateExecuted = true; await Task.CompletedTask; });
        _timeProvider.Advance(_delay);
        await task;

        userDelegateExecuted.Should().BeTrue();
        var after = _timeProvider.GetUtcNow();
        (after - before).Seconds.Should().Be(0);
        onLatencyExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_when_it_was_cancelled_running_the_strategy()
    {
        var userDelegateExecuted = false;

        using var cts = new CancellationTokenSource();
        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Randomizer = () => 0.5;
        _options.LatencyGenerator = (_) =>
        {
            cts.Cancel();
            return new ValueTask<TimeSpan>(_delay);
        };

        var sut = CreateSut();
        await sut.Invoking(s => s.ExecuteAsync(async _ => { await Task.CompletedTask; }, cts.Token).AsTask())
            .Should()
            .ThrowAsync<OperationCanceledException>();

        userDelegateExecuted.Should().BeFalse();
    }

    private ResiliencePipeline CreateSut() => new LatencyChaosStrategy(_options, _timeProvider, _telemetry).AsPipeline();
}
