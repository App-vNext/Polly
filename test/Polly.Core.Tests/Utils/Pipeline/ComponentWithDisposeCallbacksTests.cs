using NSubstitute;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ComponentWithDisposeCallbacksTests
{
    [Fact]
    public async Task Dispose_Ok()
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
        await sut.DisposeAsync();
        await sut.DisposeAsync();
        await component.Received(2).DisposeAsync();

        // Assert
        callbacks.ShouldBeEmpty();
        called1.ShouldBe(1);
        called2.ShouldBe(1);
    }
}
