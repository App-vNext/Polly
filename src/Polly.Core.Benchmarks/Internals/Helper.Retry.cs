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
                    .HandleResult(Failure)
                    .Or<InvalidOperationException>()
                    .WaitAndRetryAsync(3, attempt => delay, (_, _) => Task.CompletedTask),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                var options = new RetryStrategyOptions<string>
                {
                    RetryCount = 3,
                    BackoffType = RetryBackoffType.Constant,
                    BaseDelay = delay
                };

                options.ShouldRetry.HandleOutcome((outcome, _) => outcome.Result == Failure || outcome.Exception is InvalidOperationException);
                options.OnRetry.Register(() => { });

                builder.AddRetry(options);
            }),
            _ => throw new NotSupportedException()
        };
    }
}
