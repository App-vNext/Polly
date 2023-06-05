using System;
using Polly.CircuitBreaker;
using Polly.Strategy;

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreateOpenedCircuitBreaker(PollyVersion version, bool handleOutcome)
    {
        var manualControl = new CircuitBreakerManualControl();
        var options = new AdvancedCircuitBreakerStrategyOptions
        {
            ShouldHandle = _ => PredicateResult.True,
            ManualControl = manualControl,
        };

        if (version == PollyVersion.V8)
        {
            var builder = new ResilienceStrategyBuilder();

            if (handleOutcome)
            {
                builder.AddStrategy(new OutcomeHandlingStrategy());
            }

            var strategy = builder.AddAdvancedCircuitBreaker(options).Build();
            manualControl.IsolateAsync().GetAwaiter().GetResult();
            return strategy;
        }
        else
        {
            var policy = Policy.HandleResult<string>(r => true).AdvancedCircuitBreakerAsync(options.FailureThreshold, options.SamplingDuration, options.MinimumThroughput, options.BreakDuration);
            policy.Isolate();
            return policy;
        }
    }

    public static object CreateCircuitBreaker(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy
                    .HandleResult(Failure)
                    .Or<InvalidOperationException>()
                    .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 10, TimeSpan.FromSeconds(5)),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                builder.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions<string>
                {
                    FailureThreshold = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(5),
                    ShouldHandle = args => args switch
                    {
                        { Exception: InvalidOperationException } => PredicateResult.True,
                        { Result: string result } when result == Failure => PredicateResult.True,
                        _ => PredicateResult.False
                    }
                });
            }),
            _ => throw new NotSupportedException()
        };
    }

    private class OutcomeHandlingStrategy : ResilienceStrategy
    {
        protected override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            var result = await callback(context, state).ConfigureAwait(false);

            if (result.Exception is not null)
            {
                return new Outcome<TResult>(default(TResult)!);
            }

            return result;
        }
    }

}
