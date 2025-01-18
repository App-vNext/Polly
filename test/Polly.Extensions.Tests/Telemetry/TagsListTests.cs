using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

[Collection(nameof(NonParallelizableCollection))]
public class TagsListTests
{
    [Fact]
    public async Task Pooling_OK() =>
        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            var context = TagsList.Get();

            TagsList.Return(context);

            TagsList.Get().ShouldBeSameAs(context);
        });
}
