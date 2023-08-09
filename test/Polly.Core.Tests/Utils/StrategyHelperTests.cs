using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class StrategyHelperTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ExecuteCallbackSafeAsync_CallbackThrows_EnsureExceptionWrapped(bool isAsync)
    {
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
}
