#pragma warning disable S4225 // Extension methods should not extend "object"

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static async ValueTask ExecuteAsync(this object obj, PollyVersion version)
    {
        switch (version)
        {
            case PollyVersion.V7:
                await ((IAsyncPolicy<int>)obj).ExecuteAsync(static _ => Task.FromResult(999), CancellationToken.None).ConfigureAwait(false);
                return;
            case PollyVersion.V8:
                await ((ResilienceStrategy)obj).ExecuteValueTaskAsync(static _ => new ValueTask<int>(999), CancellationToken.None).ConfigureAwait(false);
                return;
        }

        throw new NotSupportedException();
    }

    private static ResilienceStrategy CreateStrategy(Action<ResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();
        configure(builder);
        return builder.Build();
    }
}
