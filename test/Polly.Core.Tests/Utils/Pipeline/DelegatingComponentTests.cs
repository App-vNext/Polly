using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public static class DelegatingComponentTests
{
    [Fact]
    public static async Task ExecuteComponent_ReturnsCorrectResult()
    {
        await using var component = new CallbackComponent();
        var next = new CallbackComponent();
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var state = 1;

        await using var delegating = new DelegatingComponent(component) { Next = next };

        var actual = await delegating.ExecuteComponent(
            async static (_, state) => await Outcome.FromResultAsValueTask(state + 1),
            context,
            state);

        actual.Result.ShouldBe(2);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public static async Task ExecuteComponentAot_ReturnsCorrectResult()
    {
        await using var component = new CallbackComponent();
        var next = new CallbackComponent();
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var state = 1;

        await using var delegating = new DelegatingComponent(component) { Next = next };

        var actual = await delegating.ExecuteComponentAot(
            async static (_, state) => await Outcome.FromResultAsValueTask(state + 1),
            context,
            state);

        actual.Result.ShouldBe(2);
    }
#endif

    private sealed class CallbackComponent : PipelineComponent
    {
        public override ValueTask DisposeAsync() => default;

        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state) => callback(context, state);
    }
}
