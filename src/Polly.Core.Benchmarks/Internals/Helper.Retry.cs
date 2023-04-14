#pragma warning disable S4225 // Extension methods should not extend "object"

using System;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static object CreateRetry(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy
                    .HandleResult(10)
                    .Or<InvalidOperationException>()
                    .WaitAndRetryAsync(3, attempt => delay, (_, _) => Task.CompletedTask),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                var options = new RetryStrategyOptions
                {
                    RetryCount = 3,
                    BackoffType = RetryBackoffType.Constant,
                    BaseDelay = delay
                };

                options.ShouldRetry.HandleOutcome<int>((outcome, _) => outcome.Result == 10 || outcome.Exception is InvalidOperationException);
                options.OnRetry.Register<int>(() => { });

                builder.AddRetry(options);
            }),
            _ => throw new NotSupportedException()
        };
    }
}
