using NSubstitute;
using Polly.Registry;
using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ConcurrentRegistryInitialization_DisposesDiscardedPipelines(bool generic, bool reload)
    {
        const int Callers = 4;
        using var barrier = new Barrier(Callers);
        using var firstReload = new CancellationTokenSource();
        using var secondReload = new CancellationTokenSource();
        await using var registry = new ResiliencePipelineRegistry<string>();
        var builds = 0;
        var disposals = 0;

        void Configure(ResiliencePipelineBuilderBase builder, ConfigureBuilderContext<string> context)
        {
            var build = Interlocked.Increment(ref builds);
            context.OnPipelineDisposed(() => Interlocked.Increment(ref disposals));
            if (reload)
            {
                context.AddReloadToken(firstReload.IsCancellationRequested ? secondReload.Token : firstReload.Token);
            }

            // All callers must enter configuration before any candidate can be cached.
            if (build <= Callers)
            {
                barrier.SignalAndWait(TestCancellation.Token);
            }
        }

        var tasks = Enumerable.Range(0, Callers).Select(_ => Task.Factory.StartNew(
            () => GetRegistryPipeline(registry, generic, Configure),
            TestCancellation.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default)).ToArray();
        var pipelines = await Task.WhenAll(tasks);

        builds.ShouldBe(Callers);
        disposals.ShouldBe(Callers - 1);
        pipelines.ShouldAllBe(pipeline => ReferenceEquals(pipeline, pipelines[0]));
        GetRegistryPipeline(registry, generic, (_, _) => throw new InvalidOperationException("Already cached"))
            .ShouldBeSameAs(pipelines[0]);

        if (generic)
        {
            ((ResiliencePipeline<int>)pipelines[0]).Execute(() => 42).ShouldBe(42);
        }
        else
        {
            ((ResiliencePipeline)pipelines[0]).Execute(() => 42).ShouldBe(42);
        }

        firstReload.Cancel();
        builds.ShouldBe(reload ? Callers + 1 : Callers);
        disposals.ShouldBe(reload ? Callers : Callers - 1);

        await registry.DisposeAsync();
        disposals.ShouldBe(builds);
        secondReload.Cancel();
        builds.ShouldBe(reload ? Callers + 1 : Callers);
        disposals.ShouldBe(builds);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DiscardedRegistryPipeline_ReportsCallbackFailure(bool generic)
    {
        using var registry = new ResiliencePipelineRegistry<string>();
        var listener = new FakeTelemetryListener();
        var exception = new InvalidOperationException("Disposal failed");
        object? retained = null;

        var returned = GetRegistryPipeline(registry, generic, (builder, context) =>
        {
            // Install a winner while the outer candidate is still being configured.
            retained = GetRegistryPipeline(registry, generic, (_, _) => { });
            builder.TelemetryListener = listener;
            context.OnPipelineDisposed(() => throw exception);
        });

        returned.ShouldBeSameAs(retained);
        var reported = listener.Events.ShouldHaveSingleItem();
        reported.Event.EventName.ShouldBe("DisposeFailed");
        reported.Event.Severity.ShouldBe(ResilienceEventSeverity.Error);
        reported.Arguments.ShouldBeOfType<ReloadableComponent.DisposedFailedArguments>().Exception.ShouldBeSameAs(exception);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DiscardedRegistryPipeline_ObservesAsyncDisposalWithoutBlocking(bool generic, bool fail)
    {
        await using var registry = new ResiliencePipelineRegistry<string>();
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reported = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var listener = new FakeTelemetryListener(_ => reported.TrySetResult(true));
        var component = Substitute.For<PipelineComponent>();
        var exception = new InvalidOperationException("Async disposal failed");
        var started = false;
        object? retained = null;

        async ValueTask DisposeComponentAsync()
        {
            started = true;
            await release.Task;
            completed.SetResult(true);
            if (fail)
            {
                throw exception;
            }
        }

#pragma warning disable CA2012 // Configure the substitute's ValueTask-returning method.
        component.DisposeAsync().Returns(_ => DisposeComponentAsync());
#pragma warning restore CA2012

        var returned = GetRegistryPipeline(registry, generic, (builder, _) =>
        {
            retained = GetRegistryPipeline(registry, generic, (_, _) => { });
            builder.TelemetryListener = listener;
            builder.AddPipelineComponent(_ => component, new TestResilienceStrategyOptions());
        });

        returned.ShouldBeSameAs(retained);
        started.ShouldBeTrue();
        completed.Task.IsCompleted.ShouldBeFalse();
        release.SetResult(true);
        await completed.Task;

        if (fail)
        {
            await reported.Task;
            listener.Events.ShouldHaveSingleItem().Arguments
                .ShouldBeOfType<ReloadableComponent.DisposedFailedArguments>().Exception.ShouldBeSameAs(exception);
        }
        else
        {
            listener.Events.ShouldBeEmpty();
        }

        await component.Received(1).DisposeAsync();
    }

    private static object GetRegistryPipeline(
        ResiliencePipelineRegistry<string> registry,
        bool generic,
        Action<ResiliencePipelineBuilderBase, ConfigureBuilderContext<string>> configure) =>
        generic
            ? registry.GetOrAddPipeline<int>("key", (builder, context) => configure(builder, context))
            : registry.GetOrAddPipeline("key", (builder, context) => configure(builder, context));
}
