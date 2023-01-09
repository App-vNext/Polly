using Polly;
using Polly.Retry;

internal static partial class Helper
{
    public static object CreateRetries(PollyVersion technology)
    {
        var delay = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 =>
                Policy
                    .HandleResult(10)
                    .Or<InvalidOperationException>()
                    .WaitAndRetryAsync(3, attempt => delay, (result, time) => Task.CompletedTask),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                var options = new RetryStrategyOptions
                {
                    ShouldRetry = new Predicates<ShouldRetryArguments>()
                        .Add(10)
                        .AddException<InvalidOperationException>(),
                    RetryCount = 3,
                    RetryDelayGenerator = attemt => delay,
                    OnRetry = new Events<RetryActionArguments>().Add((args) => default(ValueTask))
                };

                builder.AddRetry(options);
            }),
            _ => throw new NotImplementedException()
        };
    }
}
