using NSubstitute;
using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ReloadablePipelineComponentTests : IDisposable
{
    private readonly FakeTelemetryListener _listener;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<bool> _synchronousFlags = [];
    private CancellationTokenSource _cancellationTokenSource;

    public ReloadablePipelineComponentTests()
    {
        _listener = new FakeTelemetryListener(args => _synchronousFlags.Add(args.Context.IsSynchronous));
        _telemetry = TestUtilities.CreateResilienceTelemetry(_listener);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public async Task Ctor_Ok()
    {
        var component = Substitute.For<PipelineComponent>();
        await using var sut = CreateSut(component);

        sut.Component.ShouldBe(component);

        ReloadableComponent.ReloadFailedEvent.ShouldBe("ReloadFailed");
    }

    [Fact]
    public async Task DisposeAsync_ComponentDisposed()
    {
        var component = Substitute.For<PipelineComponent>();
        await CreateSut(component).DisposeAsync();
        await component.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ChangeTriggered_StrategyReloaded()
    {
        var component = Substitute.For<PipelineComponent>();
        await using var sut = CreateSut(component);

        sut.Component.ShouldBe(component);
        _cancellationTokenSource.Cancel();
        sut.Component.ShouldNotBe(component);

        _listener.Events.Count(e => e.Event.EventName == "ReloadFailed").ShouldBe(0);
        _listener.Events.Count(e => e.Event.EventName == "OnReload").ShouldBe(1);
    }

    [Fact]
    public async Task ChangeTriggered_EnsureOldStrategyDisposed()
    {
        var telemetry = TestUtilities.CreateResilienceTelemetry(_listener);
        var component = Substitute.For<PipelineComponent>();
        await using var sut = CreateSut(component, () => new(Substitute.For<PipelineComponent>(), [], telemetry));

        for (var i = 0; i < 10; i++)
        {
            var src = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            src.Cancel();
            await component.Received(1).DisposeAsync();
            await sut.Component.Received(0).DisposeAsync();
        }
    }

    [Fact]
    public async Task ChangeTriggered_FactoryError_LastStrategyUsedAndErrorReported()
    {
        var component = Substitute.For<PipelineComponent>();
        await using var sut = CreateSut(component, () => throw new InvalidOperationException());

        _cancellationTokenSource.Cancel();

        sut.Component.ShouldBe(component);
        _listener.Events.Count.ShouldBe(2);

        _listener.Events[0]
            .Arguments
            .ShouldBeOfType<ReloadableComponent.OnReloadArguments>();

        var args = _listener.Events[1]
            .Arguments
            .ShouldBeOfType<ReloadableComponent.ReloadFailedArguments>();

        args.Exception.ShouldBeOfType<InvalidOperationException>();

        _synchronousFlags.ShouldAllBe(f => f);
    }

    [Fact]
    public async Task DisposeError_EnsureReported()
    {
        var component = Substitute.For<PipelineComponent>();
#pragma warning disable CA2012 // Use ValueTasks correctly
        component
            .When(c => c.DisposeAsync())
            .Do(_ => throw new InvalidOperationException());
#pragma warning restore CA2012 // Use ValueTasks correctly

        await using var sut = CreateSut(component);

        _cancellationTokenSource.Cancel();

        _listener.Events.Count.ShouldBe(2);

        _listener.Events[0]
            .Arguments
            .ShouldBeOfType<ReloadableComponent.OnReloadArguments>();

        var args = _listener.Events[1]
            .Arguments
            .ShouldBeOfType<ReloadableComponent.DisposedFailedArguments>();

        args.Exception.ShouldBeOfType<InvalidOperationException>();

        _synchronousFlags.Count.ShouldBe(2);
        _synchronousFlags[0].ShouldBeTrue();
        _synchronousFlags[1].ShouldBeFalse();
    }

    private ReloadableComponent CreateSut(PipelineComponent? initial = null, Func<ReloadableComponent.Entry>? factory = null)
    {
        factory ??= () => new ReloadableComponent.Entry(PipelineComponent.Empty, [], _telemetry);

        return (ReloadableComponent)PipelineComponentFactory.CreateReloadable(
            new ReloadableComponent.Entry(initial ?? PipelineComponent.Empty, [_cancellationTokenSource.Token], _telemetry),
            factory);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
