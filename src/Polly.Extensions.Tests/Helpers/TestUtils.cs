using Microsoft.Extensions.Logging;
using Moq;
using Polly.Extensions.Telemetry;
using Polly.Strategy;

namespace Polly.Extensions.Tests.Helpers;

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

    public static ILoggerFactory CreateLoggerFactory(out FakeLogger logger)
    {
        logger = new FakeLogger();
        var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
        loggerFactory.Setup(v => v.CreateLogger("Polly")).Returns(logger);
        loggerFactory.Setup(v => v.Dispose());

        return loggerFactory.Object;
    }

    public static TestStrategy AddStrategyAndEnableTelemetry(this ResilienceStrategyBuilder builder, bool noOutcome, Action<ResilienceStrategyTelemetryOptions> configure)
    {
        var options = new ResilienceStrategyTelemetryOptions();
        configure.Invoke(options);
        builder.EnableTelemetry(options);
        builder.AddStrategy(c => new TestStrategy(c.Telemetry, noOutcome), new ResilienceStrategyOptions { StrategyName = "strategy-name", StrategyType = "strategy-type" });
        return (TestStrategy)builder.Build();
    }
}
