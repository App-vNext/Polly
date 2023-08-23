using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class PipelineComponentFactoryTests
{
    [Fact]
    public void WithDisposableCallbacks_NoCallbacks_ReturnsOriginalComponent()
    {
        var component = Substitute.For<PipelineComponent>();
        var result = PipelineComponentFactory.WithDisposableCallbacks(component, new List<Action>());
        result.Should().BeSameAs(component);
    }

    [Fact]
    public void PipelineComponentFactory_Should_Return_WrapperComponent_With_Callbacks()
    {
        var component = Substitute.For<PipelineComponent>();
        List<Action> callbacks = new List<Action> { () => { } };

        var result = PipelineComponentFactory.WithDisposableCallbacks(component, callbacks);

        result.Should().BeOfType<ComponentWithDisposeCallbacks>();
    }
}
