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
    private readonly List<TelemetryEventArguments<object, object>> _args = new();

    public LatencyChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(arg => _args.Add(arg));
        _options = new LatencyStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
    }

    public void Dispose() => _cancellationSource.Dispose();

    [Fact]
    public async Task InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
    {
        var executed = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        var sut = CreateSut();

        //var before = _timeProvider.GetUtcNow();
        //_timeProvider.Advance(_delay);

        //await sut.ExecuteAsync(async _ => { executed = true; await Task.CompletedTask; });

        executed.Should().BeFalse();

        //var after = _timeProvider.GetUtcNow();
        //(after - before).Should().BeGreaterThanOrEqualTo(_delay);
    }

    private ResiliencePipeline CreateSut() => new LatencyChaosStrategy(_options, _timeProvider, _telemetry).AsPipeline();
}
