using System.Threading.Tasks;
using Polly.Extensions.Telemetry;
using Polly.Extensions.Tests.Helpers;

namespace Polly.Extensions.Tests.Telemetry;

public class EnrichmentContextTests
{
    [Fact]
    public async Task Pooling_OK()
    {
        await TestUtils.AssertWithTimeoutAsync(() =>
        {
            var context = EnrichmentContext.Get(new TestArguments(ResilienceContext.Get()), null);

            EnrichmentContext.Return(context);

            EnrichmentContext.Get(new TestArguments(ResilienceContext.Get()), null).Should().BeSameAs(context);
        });
    }
}
