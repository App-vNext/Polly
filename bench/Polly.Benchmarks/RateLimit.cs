namespace Polly.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

[Config(typeof(PollyConfig))]
public class RateLimit
{
    private static readonly Policy SyncPolicy = Policy.RateLimit(20, TimeSpan.FromSeconds(1), int.MaxValue);
    private static readonly AsyncPolicy AsyncPolicy = Policy.RateLimitAsync(20, TimeSpan.FromSeconds(1), int.MaxValue);

    [Benchmark]
    public static void RateLimit_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public static Task RateLimit_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());

    [Benchmark]
    public static int RateLimit_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public static Task<int> RateLimit_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());
}
