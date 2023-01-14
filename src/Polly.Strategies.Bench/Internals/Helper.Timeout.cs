using Polly.Timeout;
using Polly;

internal static partial class Helper
{
    public static object CreateTimeout(PollyVersion technology)
    {
        var timeout = TimeSpan.FromSeconds(30);

        return technology switch
        {
            PollyVersion.V7 => Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic, (_, _, _) => Task.CompletedTask).AsAsyncPolicy<int>(),

            PollyVersion.V8 => CreateStrategy(builder =>
            {
                builder.AddTimeout(new TimeoutStrategyOptions
                {
                    TimeoutInterval = timeout,
                    OnTimeout = new Events<OnTimeoutArguments>().Add(args => new ValueTask())
                });
            }),
            _ => throw new NotImplementedException()
        };
    }
}
