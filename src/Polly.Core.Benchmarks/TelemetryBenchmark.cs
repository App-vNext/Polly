using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Polly.Core.Benchmarks;

public class TelemetryBenchmark
{
    private ResilienceStrategy? _strategy;
    private ResilienceStrategy? _retryStrategy;

    [GlobalSetup]
    public void Prepare()
    {
        _strategy = Build(new ResilienceStrategyBuilder());
        _retryStrategy = Build(new ResilienceStrategyBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldRetry = _ => PredicateResult.True,
            RetryCount = 1,
            BaseDelay = TimeSpan.Zero,
            BackoffType = RetryBackoffType.Constant
        }));
    }

    [Params(true, false)]
    public bool Telemetry { get; set; }

    [Benchmark]
    public async ValueTask Execute()
    {
        var context = ResilienceContext.Get();
        await _strategy!.ExecuteOutcomeAsync((_, _) => new ValueTask<Outcome<string>>(new Outcome<string>("dummy")), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
    }

    [Benchmark]
    public async ValueTask Retry()
    {
        var context = ResilienceContext.Get();
        await _retryStrategy!.ExecuteOutcomeAsync((_, _) => new ValueTask<Outcome<string>>(new Outcome<string>("dummy")), context, "state").ConfigureAwait(false);
        ResilienceContext.Return(context);
    }

    private ResilienceStrategy Build(ResilienceStrategyBuilder builder)
    {
        if (Telemetry)
        {
            builder.EnableTelemetry(NullLoggerFactory.Instance);
        }

        return builder.Build();
    }
}
