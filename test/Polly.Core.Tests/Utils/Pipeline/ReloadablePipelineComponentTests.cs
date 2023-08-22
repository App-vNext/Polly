using NSubstitute;
using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ReloadablePipelineComponentTests : IDisposable
{
    private readonly List<TelemetryEventArguments<object, object>> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenSource _cancellationTokenSource;

    public ReloadablePipelineComponentTests()
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
        var component = Substitute.For<PipelineComponent>();
        using var sut = CreateSut(component);

        sut.Component.Should().Be(component);

        ReloadableComponent.ReloadFailedEvent.Should().Be("ReloadFailed");
    }

    [Fact]
    public void Dispose_ComponentDisposed()
    {
        var component = Substitute.For<PipelineComponent>();
        CreateSut(component).Dispose();
        component.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_ComponentDisposed()
    {
        var component = Substitute.For<PipelineComponent>();
        await CreateSut(component).DisposeAsync();
        await component.Received(1).DisposeAsync();
    }

    [Fact]
    public void ChangeTriggered_StrategyReloaded()
    {
        var component = Substitute.For<PipelineComponent>();
        using var sut = CreateSut(component);

        for (var i = 0; i < 10; i++)
        {
            var src = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            src.Cancel();

            sut.Component.Should().NotBe(component);
        }

        _events.Where(e => e.Event.EventName == "ReloadFailed").Should().HaveCount(0);
        _events.Where(e => e.Event.EventName == "OnReload").Should().HaveCount(10);
    }

    [Fact]
    public void ChangeTriggered_EnsureOldStrategyDisposed()
    {
        var component = Substitute.For<PipelineComponent>();
        using var sut = CreateSut(component, () => Substitute.For<PipelineComponent>());

        for (var i = 0; i < 10; i++)
        {
            var src = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            src.Cancel();
            component.Received(1).Dispose();
            sut.Component.Received(0).Dispose();
        }
    }

    [Fact]
    public void ChangeTriggered_FactoryError_LastStrategyUsedAndErrorReported()
    {
        var component = Substitute.For<PipelineComponent>();
        using var sut = CreateSut(component, () => throw new InvalidOperationException());

        _cancellationTokenSource.Cancel();

        sut.Component.Should().Be(component);
        _events.Should().HaveCount(2);

        _events[0]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.OnReloadArguments>();

        var args = _events[1]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.ReloadFailedArguments>()
            .Subject;

        args.Exception.Should().BeOfType<InvalidOperationException>();
    }

    private ReloadableComponent CreateSut(PipelineComponent? initial = null, Func<PipelineComponent>? factory = null)
    {
        factory ??= () => PipelineComponent.Empty;

        return (ReloadableComponent)PipelineComponentFactory.CreateReloadable(initial ?? PipelineComponent.Empty,
            () => _cancellationTokenSource.Token,
            factory,
            _telemetry);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
