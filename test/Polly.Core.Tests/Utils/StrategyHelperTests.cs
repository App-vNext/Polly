using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public static class StrategyHelperTests
{
    [Fact]
    public static async Task ExecuteCallbackSafeAsync_Cancelled_EnsureOperationCanceledException()
    {
        using var token = new CancellationTokenSource();
        token.Cancel();

        var outcome = await StrategyHelper.ExecuteCallbackSafeAsync<string, string>(
            (_, _) => throw new InvalidOperationException(),
            ResilienceContextPool.Shared.Get(token.Token),
            "dummy");

        outcome.Exception.ShouldBeOfType<OperationCanceledException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static async Task ExecuteCallbackSafeAsync_CallbackThrows_EnsureExceptionWrapped(bool isAsync) =>
        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var outcome = await StrategyHelper.ExecuteCallbackSafeAsync<string, string>(
                async (_, _) =>
                {
                    if (isAsync)
                    {
                        await Task.Delay(15);
                    }

                    throw new InvalidOperationException();
                },
                ResilienceContextPool.Shared.Get(),
                "dummy");

            outcome.Exception.ShouldBeOfType<InvalidOperationException>();
        });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static async Task ExecuteCallbackSafeAsync_AsyncCallback_CompletedOk(bool isAsync) =>
        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var outcomeTask = StrategyHelper.ExecuteCallbackSafeAsync(
                async (_, _) =>
                {
                    if (isAsync)
                    {
                        await Task.Delay(15);
                    }

                    return Outcome.FromResult("success");
                },
                ResilienceContextPool.Shared.Get(),
                "dummy");

            outcomeTask.IsCompleted.ShouldBe(!isAsync);
            (await outcomeTask).Result.ShouldBe("success");
        });
}
