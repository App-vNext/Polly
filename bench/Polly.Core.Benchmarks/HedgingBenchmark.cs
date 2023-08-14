namespace Polly.Core.Benchmarks;

public class HedgingBenchmark
{
    private ResiliencePipeline<string>? _pipeline;

    [GlobalSetup]
    public void Setup() => _pipeline = Helper.CreateHedging();

    [Benchmark(Baseline = true)]
    public async ValueTask Hedging_Primary()
        => await _pipeline!.ExecuteAsync(static _ => new ValueTask<string>("primary")).ConfigureAwait(false);

    [Benchmark]
    public async ValueTask Hedging_Secondary()
        => await _pipeline!.ExecuteAsync(static _ => new ValueTask<string>(Helper.Failure)).ConfigureAwait(false);

    [Benchmark]
    public async ValueTask Hedging_Primary_AsyncWork()
        => await _pipeline!.ExecuteAsync(
            static async _ =>
            {
                await Task.Yield();
                return "primary";
            }).ConfigureAwait(false);

    [Benchmark]
    public async ValueTask Hedging_Secondary_AsyncWork()
        => await _pipeline!.ExecuteAsync(
            static async _ =>
            {
                await Task.Yield();
                return Helper.Failure;
            }).ConfigureAwait(false);
}
