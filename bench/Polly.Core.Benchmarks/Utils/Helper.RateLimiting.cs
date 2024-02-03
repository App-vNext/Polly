using System.Threading.RateLimiting;

namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreateRateLimiter(PollyVersion technology)
    {
        var timeout = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 => Policy.BulkheadAsync<string>(10, 10),
            PollyVersion.V8 => CreateStrategy(builder => builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 10
            })),
            _ => throw new NotSupportedException()
        };
    }
}
