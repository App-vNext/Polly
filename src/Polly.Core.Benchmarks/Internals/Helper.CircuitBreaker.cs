using System;
using Polly.CircuitBreaker;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
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
                var options = new AdvancedCircuitBreakerStrategyOptions<string>
                {
                    FailureThreshold = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(5),
                };

                options.ShouldHandle.HandleOutcome((outcome, _) => outcome.Result == Failure || outcome.Exception is InvalidOperationException);
                builder.AddAdvancedCircuitBreaker(options);
            }),
            _ => throw new NotSupportedException()
        };
    }
}
