#pragma warning disable S4225 // Extension methods should not extend "object"

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
                    .HandleResult(10)
                    .Or<InvalidOperationException>()
                    .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 10, TimeSpan.FromSeconds(5)),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                var options = new AdvancedCircuitBreakerStrategyOptions
                {
                    FailureThreshold = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(5),
                };

                options.ShouldHandle.HandleOutcome<int>((outcome, _) => outcome.Result == 10 || outcome.Exception is InvalidOperationException);
                builder.AddAdvancedCircuitBreaker(options);
            }),
            _ => throw new NotSupportedException()
        };
    }
}
