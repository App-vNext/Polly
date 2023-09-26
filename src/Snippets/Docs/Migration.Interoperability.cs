using System.Threading.RateLimiting;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Interoperability()
    {
        #region migration-interoperability

        // First, create a resilience pipeline.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRateLimiter(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 100
            }))
            .Build();

        // Now, convert it to a v7 policy. Note that it can be converted to both sync and async policies.
        ISyncPolicy syncPolicy = pipeline.AsSyncPolicy();
        IAsyncPolicy asyncPolicy = pipeline.AsAsyncPolicy();

        // Finally, use it in a policy wrap.
        ISyncPolicy wrappedPolicy = Policy.Wrap(
            syncPolicy,
            Policy.Handle<SomeExceptionType>().Retry(3));

        #endregion
    }
}
