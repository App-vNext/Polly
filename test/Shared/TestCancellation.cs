using Xunit;

namespace Polly;

internal static class TestCancellation
{
    public static CancellationToken Token => TestContext.Current.CancellationToken;
}
