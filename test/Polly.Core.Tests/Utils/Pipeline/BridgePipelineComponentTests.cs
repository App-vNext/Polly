using NSubstitute;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class BridgePipelineComponentTests
{
    [Fact]
    public void Ctor_Ok() =>
        new Strategy<string>(args => { }).Should().NotBeNull();

    [Fact]
    public void Execute_NonGeneric_Ok()
    {
        var values = new List<object?>();

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<object>(outcome =>
        {
            values.Add(outcome.Result);
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(args => "dummy");
        pipeline.Execute(args => 0);
        pipeline.Execute<object?>(args => null);
        pipeline.Execute(args => true);

        values[0].Should().Be("dummy");
        values[1].Should().Be(0);
        values[2].Should().BeNull();
        values[3].Should().Be(true);
    }

    [Fact]
    public void Execute_Generic_Ok()
    {
        var values = new List<object?>();

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<string>(outcome =>
        {
            values.Add(outcome.Result);
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(args => "dummy");

        values.Should().HaveCount(1);
        values[0].Should().Be("dummy");
    }

    [Fact]
    public void Pipeline_TypeCheck_Ok()
    {
        var called = false;

        var pipeline = new ResiliencePipeline(PipelineComponentFactory.FromStrategy(new Strategy<object>(outcome =>
        {
            outcome.Result.Should().Be(-1);
            called = true;
        })), DisposeBehavior.Allow, null);

        pipeline.Execute(() => -1);

        called.Should().BeTrue();
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

    private class Strategy<T> : ResilienceStrategy<T>
    {
        private readonly Action<Outcome<T>> _onOutcome;

        public Strategy(Action<Outcome<T>> onOutcome) => _onOutcome = onOutcome;

        protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
        {
            var outcome = await callback(context, state);
            _onOutcome(outcome);
            return outcome;
        }
    }
}
