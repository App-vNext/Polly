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

    [InlineData(true)]
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
}
