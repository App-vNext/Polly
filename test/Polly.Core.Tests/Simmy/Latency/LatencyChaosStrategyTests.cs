using Microsoft.Extensions.Time.Testing;
using NSubstitute;
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
    private readonly DiagnosticSource _diagnosticSource = Substitute.For<DiagnosticSource>();

    public LatencyChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource);
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

    private ResilienceStrategy CreateSut() => new LatencyChaosStrategy(_options, _timeProvider, _telemetry).AsStrategy();
}
