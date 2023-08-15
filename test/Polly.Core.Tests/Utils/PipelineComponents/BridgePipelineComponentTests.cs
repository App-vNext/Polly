using Polly.Utils;

namespace Polly.Core.Tests.Utils.PipelineComponents;

public class BridgePipelineComponentTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new Strategy<string>(args => { }).Should().NotBeNull();
    }

    [Fact]
    public void Execute_NonGeneric_Ok()
    {
        var values = new List<object?>();

        var pipeline = new ResiliencePipeline(PipelineComponent.FromStrategy(new Strategy<object>(outcome =>
        {
            values.Add(outcome.Result);
        })));

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

        var pipeline = new ResiliencePipeline(PipelineComponent.FromStrategy(new Strategy<string>(outcome =>
        {
            values.Add(outcome.Result);
        })));

        pipeline.Execute(args => "dummy");

        values.Should().HaveCount(1);
        values[0].Should().Be("dummy");
    }

    [Fact]
    public void Pipeline_TypeCheck_Ok()
    {
        var called = false;

        var pipeline = new ResiliencePipeline(PipelineComponent.FromStrategy(new Strategy<object>(outcome =>
        {
            outcome.Result.Should().Be(-1);
            called = true;
        })));

        pipeline.Execute(() => -1);

        called.Should().BeTrue();
    }

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
