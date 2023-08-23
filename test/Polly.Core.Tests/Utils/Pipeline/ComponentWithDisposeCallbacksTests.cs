using NSubstitute;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ComponentWithDisposeCallbacksTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_Ok(bool isAsync)
    {
        // Arrange
        var called1 = 0;
        var called2 = 0;

        var callbacks = new List<Action>
        {
            () => called1++,
            () => called2++
        };
        var component = Substitute.For<PipelineComponent>();
        var sut = new ComponentWithDisposeCallbacks(component, callbacks);

        // Act
        if (isAsync)
        {
            await sut.DisposeAsync();
            await sut.DisposeAsync();
            await component.Received(2).DisposeAsync();
        }
        else
        {
            sut.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
            sut.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once
            component.Received(2).Dispose();
        }

        // Assert
        callbacks.Should().HaveCount(2);
        called1.Should().Be(1);
        called2.Should().Be(1);
    }
}
