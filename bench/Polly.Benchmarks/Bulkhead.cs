namespace Polly.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

[Config(typeof(PollyConfig))]
public class Bulkhead
{
    private static readonly Policy SyncPolicy = Policy.Bulkhead(2);
    private static readonly AsyncPolicy AsyncPolicy = Policy.BulkheadAsync(2);

    [Benchmark]
    public static void Bulkhead_Synchronous() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public static Task Bulkhead_Asynchronous() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());

    [Benchmark]
    public static Task Bulkhead_Asynchronous_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public static int Bulkhead_Synchronous_With_Result() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public static Task<int> Bulkhead_Asynchronous_With_Result() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());

    [Benchmark]
    public static Task<int> Bulkhead_Asynchronous_With_Result_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);
}
