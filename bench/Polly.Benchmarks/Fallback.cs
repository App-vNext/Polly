namespace Polly.Benchmarks;

[Config(typeof(PollyConfig))]
public class Fallback
{
    private static readonly Policy<int> SyncPolicy = Policy<int>.Handle<InvalidOperationException>().Fallback(0);
    private static readonly AsyncPolicy<int> AsyncPolicy = Policy<int>.Handle<InvalidOperationException>().FallbackAsync(0);

    [Benchmark]
    public int Fallback_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public Task<int> Fallback_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());

    [Benchmark]
    public int Fallback_Synchronous_Throws() =>
        SyncPolicy.Execute(() => Workloads.FuncThrows<int, InvalidOperationException>());

    [Benchmark]
    public Task<int> Fallback_Asynchronous_Throws() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncThrowsAsync<int, InvalidOperationException>());
}
