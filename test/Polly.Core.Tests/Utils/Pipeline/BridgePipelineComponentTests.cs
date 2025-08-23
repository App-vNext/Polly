using NSubstitute;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class BridgePipelineComponentTests
{
    [Fact]
    public void Ctor_Ok() =>
        new Strategy<string>(args => { }).ShouldNotBeNull();

    [Fact]
    public void Execute_NonGeneric_Ok()
    {
        var cancellationToken = TestCancellation.Token;
        var values = new List<object?>();

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<object>(outcome =>
        {
            values.Add(outcome.Result);
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(args => "dummy", cancellationToken);
        pipeline.Execute(args => 0, cancellationToken);
        pipeline.Execute<object?>(args => null, cancellationToken);
        pipeline.Execute(args => true, cancellationToken);

        values[0].ShouldBe("dummy");
        values[1].ShouldBe(0);
        values[2].ShouldBeNull();
        values[3].ShouldBe(true);
    }

    [Fact]
    public void Execute_Generic_Ok()
    {
        var values = new List<object?>();

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<string>(outcome =>
        {
            values.Add(outcome.Result);
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(args => "dummy", TestCancellation.Token);

        values.Count.ShouldBe(1);
        values[0].ShouldBe("dummy");
    }

    [Fact]
    public void Pipeline_TypeCheck_Ok()
    {
        var called = false;

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<object>(outcome =>
        {
            outcome.Result.ShouldBe(-1);
            called = true;
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(() => -1);

        called.ShouldBeTrue();
    }

#pragma warning disable S1944 // Invalid casts should be avoided
    [Fact]
    public async Task Dispose_EnsureStrategyDisposed()
    {
        var strategy = Substitute.For<ResilienceStrategy, IDisposable>();
        await Dispose(PipelineComponentFactory.FromStrategy(strategy));
        ((IDisposable)strategy).Received(1).Dispose();

        strategy = Substitute.For<ResilienceStrategy, IAsyncDisposable>();
        await Dispose(PipelineComponentFactory.FromStrategy(strategy));
        await ((IAsyncDisposable)strategy).Received(1).DisposeAsync();
    }

    [Fact]
    public async Task Dispose_Generic_EnsureStrategyDisposed()
    {
        var strategy = Substitute.For<ResilienceStrategy<string>, IDisposable>();
        await Dispose(PipelineComponentFactory.FromStrategy(strategy));
        ((IDisposable)strategy).Received(1).Dispose();

        strategy = Substitute.For<ResilienceStrategy<string>, IAsyncDisposable>();
        await Dispose(PipelineComponentFactory.FromStrategy(strategy));
        await ((IAsyncDisposable)strategy).Received(1).DisposeAsync();
    }
#pragma warning restore S1944 // Invalid casts should be avoided

    private static async Task Dispose(PipelineComponent component) =>
        await component.DisposeAsync();

    private class Strategy<T>(Action<Outcome<T>> onOutcome) : ResilienceStrategy<T>
    {
        protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
        {
            var outcome = await callback(context, state);
            onOutcome(outcome);
            return outcome;
        }
    }
}
