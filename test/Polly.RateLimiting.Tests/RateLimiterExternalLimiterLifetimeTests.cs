using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Polly.Registry;
using Shouldly;

namespace Polly.RateLimiting.Tests
{
    public class RateLimiterExternalLimiterLifetimeTests
    {
        [Fact]
        public async Task RegistryDispose_DoesNotDisposeExternalLimiter()
        {
            using var externalLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
                QueueLimit = 0
            });

            var registry = new ResiliencePipelineRegistry<string>();
            _ = registry.GetOrAddPipeline("ext", p => p.AddRateLimiter(externalLimiter));

            await registry.DisposeAsync();

            await Should.NotThrowAsync(async () =>
            {
                var lease = await externalLimiter.AcquireAsync(1, CancellationToken.None);
                try
                {
                    lease.IsAcquired.ShouldBeTrue();
                }
                finally
                {
                    lease.Dispose();
                }
            });
        }
    }
}
