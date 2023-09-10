using NSubstitute;
using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ReloadablePipelineComponentTests : IDisposable
{
    private readonly FakeTelemetryListener _listener;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<bool> _synchronousFlags = new();
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

        sut.Component.Should().Be(component);

        ReloadableComponent.ReloadFailedEvent.Should().Be("ReloadFailed");
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

        sut.Component.Should().Be(component);
        _cancellationTokenSource.Cancel();
        sut.Component.Should().NotBe(component);

        _listener.Events.Where(e => e.Event.EventName == "ReloadFailed").Should().HaveCount(0);
        _listener.Events.Where(e => e.Event.EventName == "OnReload").Should().HaveCount(1);
    }

    [Fact]
    public async Task ChangeTriggered_EnsureOldStrategyDisposed()
    {
        var telemetry = TestUtilities.CreateResilienceTelemetry(_listener);
        var component = Substitute.For<PipelineComponent>();
        await using var sut = CreateSut(component, () => new(Substitute.For<PipelineComponent>(), new List<CancellationToken>(), telemetry));

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

        sut.Component.Should().Be(component);
        _listener.Events.Should().HaveCount(2);

        _listener.Events[0]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.OnReloadArguments>();

        var args = _listener.Events[1]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.ReloadFailedArguments>()
            .Subject;

        args.Exception.Should().BeOfType<InvalidOperationException>();

        _synchronousFlags.Should().AllSatisfy(f => f.Should().BeTrue());
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

        _listener.Events.Should().HaveCount(2);

        _listener.Events[0]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.OnReloadArguments>();

        var args = _listener.Events[1]
            .Arguments
            .Should()
            .BeOfType<ReloadableComponent.DisposedFailedArguments>()
            .Subject;

        args.Exception.Should().BeOfType<InvalidOperationException>();

        _synchronousFlags.Should().HaveCount(2);
        _synchronousFlags[0].Should().BeTrue();
        _synchronousFlags[1].Should().BeFalse();
    }

    private ReloadableComponent CreateSut(PipelineComponent? initial = null, Func<ReloadableComponent.Entry>? factory = null)
    {
        factory ??= () => new ReloadableComponent.Entry(PipelineComponent.Empty, new List<CancellationToken>(), _telemetry);

        return (ReloadableComponent)PipelineComponentFactory.CreateReloadable(
            new ReloadableComponent.Entry(initial ?? PipelineComponent.Empty, new List<CancellationToken> { _cancellationTokenSource.Token }, _telemetry),
            factory);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
