#pragma warning disable S4225 // Extension methods should not extend "object"

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    private static readonly HttpResponseMessage ResponseMessage = new();

    public static async ValueTask ExecuteAsync(this object obj, PollyVersion version)
    {
        switch (version)
        {
            case PollyVersion.V7:
                await ((IAsyncPolicy<HttpResponseMessage>)obj).ExecuteAsync(static _ => Task.FromResult(ResponseMessage), CancellationToken.None).ConfigureAwait(false);
                return;
            case PollyVersion.V8:
                await ((ResilienceStrategy)obj).ExecuteAsync(static _ => new ValueTask<HttpResponseMessage>(ResponseMessage), CancellationToken.None).ConfigureAwait(false);
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
