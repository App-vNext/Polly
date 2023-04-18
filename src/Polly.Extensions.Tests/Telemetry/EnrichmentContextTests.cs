using System.Threading.Tasks;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class EnrichmentContextTests
{
    [Fact]
    public async Task Pooling_OK()
    {
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            var context = EnrichmentContext.Get(ResilienceContext.Get(), null, null);

            EnrichmentContext.Return(context);

            EnrichmentContext.Get(ResilienceContext.Get(), null, null).Should().BeSameAs(context);
        });
    }
}
