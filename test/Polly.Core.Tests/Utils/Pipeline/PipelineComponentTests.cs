using System.Threading.Tasks;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class PipelineComponentTests
{
    [Fact]
    public async Task Dispose_Ok()
    {
        PipelineComponent.Empty.Should().NotBeNull();
        await PipelineComponent.Empty.DisposeAsync();
    }
}
