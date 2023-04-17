using Polly.Strategy;

namespace Polly.Extensions.Tests.Helpers;

public class TestStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly bool _noOutcome;

    public TestStrategy(ResilienceStrategyTelemetry telemetry, bool noOutcome)
    {
        _telemetry = telemetry;
        _noOutcome = noOutcome;
    }

    protected override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        if (_noOutcome)
        {
            _telemetry.Report("no-outcome", new TestArguments(context));
        }

        try
        {
            var result = await callback(context, state);
            if (!_noOutcome)
            {
                _telemetry.Report("outcome", new Outcome<TResult>(result), new TestArguments(context));
            }

            return result;
        }
        catch (Exception e)
        {
            if (!_noOutcome)
            {
                _telemetry.Report("outcome", new Outcome<TResult>(e), new TestArguments(context));
            }

            throw;
        }
    }
}
