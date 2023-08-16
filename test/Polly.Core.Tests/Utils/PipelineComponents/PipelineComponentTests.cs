using System.Threading.Tasks;
using Polly.Utils;

namespace Polly.Core.Tests.Utils.PipelineComponents;

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
