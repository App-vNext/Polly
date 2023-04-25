using Polly.Hedging;

namespace Polly;

internal sealed class HedgingResilienceStrategy : ResilienceStrategy
{
    public HedgingResilienceStrategy(HedgingStrategyOptions options)
    {
        HedgingDelay = options.HedgingDelay;
        MaxHedgedAttempts = options.MaxHedgedAttempts;
        HedgingDelayGenerator = options.HedgingDelayGenerator.CreateHandler(HedgingConstants.DefaultHedgingDelay, _ => true);
        HedgingHandler = options.Handler.CreateHandler();
    }

    public TimeSpan HedgingDelay { get; }

    public int MaxHedgedAttempts { get; }

    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; }

    public HedgingHandler.Handler? HedgingHandler { get; }

    protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return callback(context, state);
    }

    internal ValueTask<TimeSpan> GetHedgingDelayAsync(ResilienceContext context, int attempt)
    {
        if (HedgingDelayGenerator == null)
        {
            return new ValueTask<TimeSpan>(HedgingDelay);
        }

        return HedgingDelayGenerator(new HedgingDelayArguments(context, attempt));
    }

}
