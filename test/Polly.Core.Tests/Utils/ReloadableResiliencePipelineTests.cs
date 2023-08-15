using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class ReloadableResiliencePipelineTests : IDisposable
{
    private readonly List<TelemetryEventArguments<object, object>> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenSource _cancellationTokenSource;

    public ReloadableResiliencePipelineTests()
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
        var strategy = new TestResilienceStrategy().AsPipeline();
        var sut = CreateSut(strategy);

        sut.Pipeline.Should().Be(strategy);

        ReloadableResiliencePipeline.ReloadFailedEvent.Should().Be("ReloadFailed");
    }

    [Fact]
    public void ChangeTriggered_StrategyReloaded()
    {
        var strategy = new TestResilienceStrategy().AsPipeline();
        var sut = CreateSut(strategy);

        for (var i = 0; i < 10; i++)
        {
            var src = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            src.Cancel();

            sut.Pipeline.Should().NotBe(strategy);
        }

        _events.Where(e => e.Event.EventName == "ReloadFailed").Should().HaveCount(0);
        _events.Where(e => e.Event.EventName == "OnReload").Should().HaveCount(10);
    }

    [Fact]
    public void ChangeTriggered_FactoryError_LastStrategyUsedAndErrorReported()
    {
        var strategy = new TestResilienceStrategy().AsPipeline();
        var sut = CreateSut(strategy, () => throw new InvalidOperationException());

        _cancellationTokenSource.Cancel();

        sut.Pipeline.Should().Be(strategy);
        _events.Should().HaveCount(2);

        _events[0]
            .Arguments
            .Should()
            .BeOfType<ReloadableResiliencePipeline.OnReloadArguments>();

        var args = _events[1]
            .Arguments
            .Should()
            .BeOfType<ReloadableResiliencePipeline.ReloadFailedArguments>()
            .Subject;

        args.Exception.Should().BeOfType<InvalidOperationException>();
    }

    private ReloadableResiliencePipeline CreateSut(ResiliencePipeline? initial = null, Func<ResiliencePipeline>? factory = null)
    {
        factory ??= () => new TestResilienceStrategy().AsPipeline();

        return new(
            initial ?? new TestResilienceStrategy().AsPipeline(),
            () => _cancellationTokenSource.Token,
            factory,
            _telemetry);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
