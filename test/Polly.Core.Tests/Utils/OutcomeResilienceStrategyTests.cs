using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class OutcomeResilienceStrategyTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new Strategy<string>(args => { }, true).Should().NotBeNull();
        new Strategy<object>(args => { }, false).Should().NotBeNull();
        new Strategy<object>(args => { }, true).Should().NotBeNull();
    }

    [Fact]
    public void Ctor_InvalidArgs_Throws()
    {
        this.Invoking(_ => new Strategy<string>(_ => { }, false)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Execute_NonGeneric_Ok()
    {
        var values = new List<object?>();

        var strategy = new Strategy<object>(outcome =>
        {
            values.Add(outcome.Result);
        },
        false);

        strategy.Execute(args => "dummy");
        strategy.Execute(args => 0);
        strategy.Execute<object?>(args => null);
        strategy.Execute(args => true);

        values[0].Should().Be("dummy");
        values[1].Should().Be(0);
        values[2].Should().BeNull();
        values[3].Should().Be(true);
    }

    [Fact]
    public void Execute_Generic_Ok()
    {
        var values = new List<object?>();

        var strategy = new Strategy<string>(outcome =>
        {
            values.Add(outcome.Result);
        },
        true);

        strategy.Execute(args => "dummy");
        strategy.Execute(args => 0);
        strategy.Execute<object?>(args => null);
        strategy.Execute(args => true);

        values.Should().HaveCount(1);
        values[0].Should().Be("dummy");
    }

    private class Strategy<T> : OutcomeResilienceStrategy<T>
    {
        private readonly Action<Outcome<T>> _onOutcome;

        public Strategy(Action<Outcome<T>> onOutcome, bool isGeneric)
            : base(isGeneric) => _onOutcome = onOutcome;

        protected override async ValueTask<Outcome<T>> ExecuteCallbackAsync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
        {
            var outcome = await callback(context, state);
            _onOutcome(outcome);
            return outcome;
        }
    }

}
