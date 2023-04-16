using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Core.Tests.Helpers;

#pragma warning disable CA1031 // Do not catch general exception types

public static class TestUtils
{
    public static Task AssertWithTimeoutAsync(Func<Task> assertion) => AssertWithTimeoutAsync(assertion, TimeSpan.FromSeconds(60));

    public static Task AssertWithTimeoutAsync(Action assertion) => AssertWithTimeoutAsync(
        () =>
        {
            assertion();
            return Task.CompletedTask;
        },
        TimeSpan.FromSeconds(60));

    public static async Task AssertWithTimeoutAsync(Func<Task> assertion, TimeSpan timeout)
    {
        var watch = Stopwatch.StartNew();

        while (true)
        {
            try
            {
                await assertion();
                return;
            }
            catch (Exception) when (watch.Elapsed < timeout)
            {
                await Task.Delay(5);
            }
        }
    }

    public static ResilienceStrategyTelemetry CreateResilienceTelemetry(DiagnosticSource source)
        => new(new ResilienceTelemetrySource("dummy-builder", new ResilienceProperties(), "strategy-name", "strategy-type"), source);
}
