using System.Collections.Generic;
using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Core.Tests.Utils;

public class ReloadableResilienceStrategyTests : IDisposable
{
    private readonly List<TelemetryEventArguments> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenSource _cancellationTokenSource;

    public ReloadableResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(args =>
        {
            args.Context.IsSynchronous.Should().BeTrue();
            args.Context.IsVoid.Should().BeTrue();
            _events.Add(args);
        });

        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public void Ctor_Ok()
    {
        var strategy = new TestResilienceStrategy();
        var sut = CreateSut(strategy);

        sut.Strategy.Should().Be(strategy);

        ReloadableResilienceStrategy.StrategyType.Should().Be("ReloadableStrategy");
        ReloadableResilienceStrategy.StrategyName.Should().Be("ReloadableStrategy");
        ReloadableResilienceStrategy.ReloadFailedEvent.Should().Be("ReloadFailed");
    }

    [Fact]
    public void ChangeTriggered_StrategyReloaded()
    {
        var strategy = new TestResilienceStrategy();
        var sut = CreateSut(strategy);

        for (var i = 0; i < 10; i++)
        {
            var src = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            src.Cancel();

            sut.Strategy.Should().NotBe(strategy);
        }

        _events.Should().HaveCount(0);
    }

    [Fact]
    public void ChangeTriggered_FactoryError_LastStrategyUsedAndErrorReported()
    {
        var strategy = new TestResilienceStrategy();
        var sut = CreateSut(strategy, () => throw new InvalidOperationException());

        _cancellationTokenSource.Cancel();

        sut.Strategy.Should().Be(strategy);
        _events.Should().HaveCount(1);
        var args = _events[0]
            .Arguments
            .Should()
            .BeOfType<ReloadableResilienceStrategy.ReloadFailedArguments>()
            .Subject;

        args.Exception.Should().BeOfType<InvalidOperationException>();
    }

    private ReloadableResilienceStrategy CreateSut(ResilienceStrategy? initial = null, Func<ResilienceStrategy>? factory = null)
    {
        factory ??= () => new TestResilienceStrategy();

        return new(
            initial ?? new TestResilienceStrategy(),
            () => _cancellationTokenSource.Token,
            factory,
            _telemetry);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
