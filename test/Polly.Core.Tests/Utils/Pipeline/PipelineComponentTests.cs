using System.Threading.Tasks;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class PipelineComponentTests
{
    [Fact]
    public async Task Dispose_Ok()
    {
        PipelineComponent.Null.Should().NotBeNull();
        PipelineComponent.Null.Dispose();
        await PipelineComponent.Null.DisposeAsync();
    }
}
