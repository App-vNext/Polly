using NSubstitute;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class PipelineComponentFactoryTests
{
#pragma warning disable IDE0028
    public static List<IEnumerable<Action>> EmptyCallbacks = new()
    {
        Array.Empty<Action>(),
        Enumerable.Empty<Action>(),
        new List<Action>(),
        new EmptyActionEnumerable(), // Explicitly does not provide TryGetNonEnumeratedCount()
    };

    public static List<IEnumerable<Action>> NonEmptyCallbacks = new()
    {
        new[] { () => { } },
        Enumerable.TakeWhile(Enumerable.Repeat(() => { }, 50), (_, i) => i < 1), // Defeat optimisation for TryGetNonEnumeratedCount()
        new List<Action> { () => { } },
    };
#pragma warning restore IDE0028

    [Theory]
#pragma warning disable xUnit1045
    //[MemberData(nameof(EmptyCallbacks))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
#pragma warning restore xUnit1045
    public void WithDisposableCallbacks_NoCallbacks_ReturnsOriginalComponent(int index)
    {
        IEnumerable<Action> callbacks = EmptyCallbacks[index];
        var component = Substitute.For<PipelineComponent>();
        var result = PipelineComponentFactory.WithDisposableCallbacks(component, callbacks);
        result.ShouldBeSameAs(component);
    }

    [Theory]
#pragma warning disable xUnit1045
    //[MemberData(nameof(NonEmptyCallbacks))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
#pragma warning restore xUnit1045
    public void PipelineComponentFactory_Should_Return_WrapperComponent_With_Callbacks(int index)
    {
        IEnumerable<Action> callbacks = NonEmptyCallbacks[index];

        var component = Substitute.For<PipelineComponent>();

        var result = PipelineComponentFactory.WithDisposableCallbacks(component, callbacks);

        result.ShouldBeOfType<ComponentWithDisposeCallbacks>();
    }

    private sealed class EmptyActionEnumerable : IEnumerable<Action>, IEnumerator<Action>
    {
        public Action Current => null!;

        object IEnumerator.Current => null!;

        public void Dispose()
        {
            // No-op
        }

        public IEnumerator<Action> GetEnumerator() => this;

        public bool MoveNext() => false;

        public void Reset()
        {
            // No-op
        }

        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
