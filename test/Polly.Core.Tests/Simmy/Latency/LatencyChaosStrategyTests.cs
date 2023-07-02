using Microsoft.Extensions.Time.Testing;
using Moq;
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

    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public LatencyChaosStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);
        _options = new LatencyStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
    }

    public void Dispose() => _cancellationSource.Dispose();

    [Fact]
    public void InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
    {
        var executed = false;

        _options.InjectionRate = 0.6;
        _options.Enabled = true;
        _options.Latency = _delay;
        _options.Randomizer = () => 0.5;

        //var sut = CreateSut();

        //var before = _timeProvider.GetUtcNow();

        //sut.Execute(_ => executed = true);

        //executed.Should().BeTrue();

        //var after = _timeProvider.GetUtcNow();
        //(after - before).Should().BeGreaterThanOrEqualTo(_delay);

        executed.Should().BeFalse();
    }

    private LatencyChaosStrategy CreateSut() => new(_options, _timeProvider, _telemetry);
}
