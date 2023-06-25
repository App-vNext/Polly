#pragma warning disable S4225 // Extension methods should not extend "object"

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static async ValueTask ExecuteAsync(this object obj, PollyVersion version)
    {
        switch (version)
        {
            case PollyVersion.V7:
                await ((IAsyncPolicy<string>)obj).ExecuteAsync(static _ => Task.FromResult("dummy"), CancellationToken.None).ConfigureAwait(false);
                return;
            case PollyVersion.V8:
                var context = ResilienceContext.Get();

                await ((ResilienceStrategy<string>)obj).ExecuteOutcomeAsync(
                    static (_, _) => Outcome.FromResultAsTask("dummy"),
                    context,
                    string.Empty).ConfigureAwait(false);

                ResilienceContext.Return(context);
                return;
        }

        throw new NotSupportedException();
    }

    private static ResilienceStrategy<string> CreateStrategy(Action<ResilienceStrategyBuilder<string>> configure)
    {
        var builder = new ResilienceStrategyBuilder<string>();
        configure(builder);
        return builder.Build();
    }
}
