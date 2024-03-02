using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class StrategyHelperTests
{
    [Fact]
    public async Task ExecuteCallbackSafeAsync_Cancelled_EnsureOperationCanceledException()
    {
        using var token = new CancellationTokenSource();
        token.Cancel();

        var outcome = await StrategyHelper.ExecuteCallbackSafeAsync<string, string>(
            (_, _) => throw new InvalidOperationException(),
            ResilienceContextPool.Shared.Get(token.Token),
            "dummy");

        outcome.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [InlineData(true, Skip = "Causes test process to abort in Release mode for net9.0.")]
    [InlineData(false)]
    [Theory]
    public async Task ExecuteCallbackSafeAsync_CallbackThrows_EnsureExceptionWrapped(bool isAsync) =>
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

            outcome.Exception.Should().BeOfType<InvalidOperationException>();
        });

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ExecuteCallbackSafeAsync_AsyncCallback_CompletedOk(bool isAsync) =>
        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var outcomeTask = StrategyHelper.ExecuteCallbackSafeAsync<string, string>(
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

            outcomeTask.IsCompleted.Should().Be(!isAsync);
            (await outcomeTask).Result.Should().Be("success");
        });
}
