using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks;

[Config(typeof(PollyConfig))]
public class NoOp
{
    private static readonly Policy SyncPolicy = Policy.NoOp();
    private static readonly AsyncPolicy AsyncPolicy = Policy.NoOpAsync();

    [Benchmark]
    public void NoOp_Synchronous()
    {
        SyncPolicy.Execute(() => Workloads.Action());
    }

    [Benchmark]
    public Task NoOp_Asynchronous()
    {
        return AsyncPolicy.ExecuteAsync((token) => Workloads.ActionAsync(token), CancellationToken.None);
    }

    [Benchmark]
    public int NoOp_Synchronous_With_Result()
    {
        return SyncPolicy.Execute(() => Workloads.Func<int>());
    }

    [Benchmark]
    public Task<int> NoOp_Asynchronous_With_Result()
    {
        return AsyncPolicy.ExecuteAsync((token) => Workloads.FuncAsync<int>(token), CancellationToken.None);
    }
}
